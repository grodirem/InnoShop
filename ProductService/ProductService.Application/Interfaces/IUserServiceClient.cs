namespace ProductService.Application.Interfaces;

public interface IUserServiceClient
{
    Task<bool> ValidateUserAsync(Guid userId);
    Task<bool> IsUserActiveAsync(Guid userId);
    Task UpdateProductsUserStatusAsync(Guid userId, bool isActive);
}