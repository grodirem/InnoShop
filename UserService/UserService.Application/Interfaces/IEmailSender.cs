namespace UserService.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string confirmationToken);
    Task SendPasswordResetAsync(string email, string resetToken);
}