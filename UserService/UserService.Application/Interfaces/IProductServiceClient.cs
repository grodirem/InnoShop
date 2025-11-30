namespace UserService.Application.Interfaces;

public interface IProductServiceClient
{
    Task UpdateProductsUserStatusAsync(Guid userId, bool isActive);
}