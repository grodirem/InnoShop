using UserService.Application.Models.DTOs;

namespace UserService.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(CreateUserDto createUserDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task ConfirmEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task<bool> ValidateTokenAsync(string token);
}