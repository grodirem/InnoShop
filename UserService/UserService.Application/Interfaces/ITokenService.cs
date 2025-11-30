using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateEmailConfirmationToken();
    string GeneratePasswordResetToken();
}