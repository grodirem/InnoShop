using ProductService.Application.Models.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByUserIdAsync(Guid userId);
    Task<List<Product>> GetByFilterAsync(ProductFilterDto filter);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
    Task SoftDeleteByUserIdAsync(Guid userId, bool isDeleted);
    Task<bool> ExistsAsync(Guid id);
}