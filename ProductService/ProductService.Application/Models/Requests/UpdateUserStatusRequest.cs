namespace ProductService.Application.Models.Requests;

public class UpdateUserStatusRequest
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}