using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationToken)
    {
        var confirmationLink = $"{_configuration["App:BaseUrl"]}/api/auth/confirm-email?token={confirmationToken}";

        var subject = "Confirm Your Email Address";
        var body = $@"
            <h2>Email Confirmation</h2>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href='{confirmationLink}'>Confirm Email</a></p>
            <p>If you didn't create an account on our website, please ignore this email.</p>
            <br>
            <p><strong>Confirmation link:</strong> {confirmationLink}</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string resetToken)
    {
        var resetLink = $"{_configuration["App:BaseUrl"]}/api/auth/reset-password?token={resetToken}";

        var subject = "Password Reset Request";
        var body = $@"
            <h2>Password Reset</h2>
            <p>To reset your password, please click the link below:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you didn't request a password reset, please ignore this email.</p>
            <br>
            <p><strong>Reset link:</strong> {resetLink}</p>
            <p><strong>This link is valid for 1 hour.</strong></p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("=== FAKE EMAIL SENDER ===");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {Body}", body);
        _logger.LogInformation("=== EMAIL WOULD BE SENT ===");

        await Task.CompletedTask;
    }
}