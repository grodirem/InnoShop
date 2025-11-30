namespace UserService.Application.Models.Requests;

public class ChangeUserStatusRequest
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}