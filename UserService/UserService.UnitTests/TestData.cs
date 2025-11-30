using UserService.Application.Models.DTOs;
using UserService.Application.Models.Requests;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using System.Security.Claims;

namespace UserService.UnitTests;

public static class TestData
{
    public static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static ClaimsPrincipal GetTestUser(Guid? userId = null, string role = "User")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, (userId ?? TestUserId).ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    public static User GetTestUserEntity(Guid? userId = null, string role = "User", UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = userId ?? TestUserId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = role,
            Status = status,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User GetAdminUserEntity(Guid? userId = null)
    {
        return new User
        {
            Id = userId ?? AdminUserId,
            Name = "Admin User",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            Status = UserStatus.Active,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateUserDto GetCreateUserDto()
    {
        return new CreateUserDto
        {
            Name = "New Test User",
            Email = "newuser@example.com",
            Password = "NewPassword123!"
        };
    }

    public static UpdateUserDto GetUpdateUserDto()
    {
        return new UpdateUserDto
        {
            Name = "Updated Test User",
            Email = "updated@example.com"
        };
    }

    public static LoginDto GetLoginDto()
    {
        return new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };
    }

    public static ConfirmEmailRequest GetConfirmEmailRequest()
    {
        return new ConfirmEmailRequest { Token = "confirmation-token" };
    }

    public static ForgotPasswordRequest GetForgotPasswordRequest()
    {
        return new ForgotPasswordRequest { Email = "test@example.com" };
    }

    public static ResetPasswordRequest GetResetPasswordRequest()
    {
        return new ResetPasswordRequest
        {
            Token = "reset-token",
            NewPassword = "NewPassword123!"
        };
    }

    public static ChangeUserStatusRequest GetChangeUserStatusRequest()
    {
        return new ChangeUserStatusRequest
        {
            UserId = TestData.OtherUserId,
            IsActive = false
        };
    }

    public static ChangeUserStatusRequest GetInvalidChangeUserStatusRequest()
    {
        return new ChangeUserStatusRequest
        {
            UserId = Guid.Empty,
            IsActive = false
        };
    }
}