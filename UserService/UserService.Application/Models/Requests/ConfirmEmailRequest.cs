namespace UserService.Application.Models.Requests;

public class ConfirmEmailRequest
{
    public string Token { get; set; } = string.Empty;
}