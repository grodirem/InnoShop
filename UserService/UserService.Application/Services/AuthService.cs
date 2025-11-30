using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using UserService.Application.Models.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<RegisterResponseDto> RegisterAsync(CreateUserDto createUserDto)
    {
        if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
        {
            throw new ArgumentException("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = "User",
            Status = UserStatus.Active,
            IsEmailConfirmed = false,
            EmailConfirmationToken = _tokenService.GenerateEmailConfirmationToken(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _emailSender.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);

        return new RegisterResponseDto
        {
            Message = "Registration successful. Please check your email to confirm your account.",
            Email = user.Email,
            UserId = user.Id
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.Status == UserStatus.Inactive)
        {
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Please confirm your email first");
        }

        var token = _tokenService.GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = MapToDto(user),
            ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"]))
        };
    }

    public async Task ConfirmEmailAsync(string token)
    {
        var user = await _userRepository.GetByEmailConfirmationTokenAsync(token);
        if (user == null)
        {
            throw new ArgumentException("Invalid confirmation token");
        }

        if (user.IsEmailConfirmed)
        {
            throw new ArgumentException("Email already confirmed");
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _userRepository.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return;
        }

        if (user.Status == UserStatus.Inactive)
        {
            throw new InvalidOperationException("Account is deactivated");
        }

        user.PasswordResetToken = _tokenService.GeneratePasswordResetToken();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user);
        await _emailSender.SendPasswordResetAsync(user.Email, user.PasswordResetToken);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(token);
        if (user == null)
        {
            throw new ArgumentException("Invalid or expired reset token");
        }

        if (user.Status == UserStatus.Inactive)
        {
            throw new InvalidOperationException("Account is deactivated");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(token);
        return user != null && user.PasswordResetTokenExpires > DateTime.UtcNow;
    }

    private UserDto MapToDto(User user) => new UserDto
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        Status = user.Status.ToString(),
        IsEmailConfirmed = user.IsEmailConfirmed,
        CreatedAt = user.CreatedAt
    };
}