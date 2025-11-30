namespace UserService.Application.Models.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}