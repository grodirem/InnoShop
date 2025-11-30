namespace UserService.Application.Models.DTOs;

public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}