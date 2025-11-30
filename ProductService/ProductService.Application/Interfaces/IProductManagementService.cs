using ProductService.Application.Models.DTOs;
using ProductService.Application.Models.Responses;

namespace ProductService.Application.Interfaces;

public interface IProductManagementService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, Guid userId);
    Task<List<ProductDto>> GetUserProductsAsync(Guid userId);
    Task<PagedResponse<ProductDto>> GetProductsByFilterAsync(ProductFilterDto filter, Guid userId);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, Guid userId);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto, Guid userId);
    Task DeleteProductAsync(Guid id, Guid userId);
    Task UpdateProductsUserStatusAsync(Guid userId, bool isActive);
}