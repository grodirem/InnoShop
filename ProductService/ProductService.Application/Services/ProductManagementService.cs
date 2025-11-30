using ProductService.Application.Interfaces;
using ProductService.Application.Models.DTOs;
using ProductService.Application.Models.Responses;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public class ProductManagementService : IProductManagementService
{
    private readonly IProductRepository _productRepository;
    private readonly IUserServiceClient _userServiceClient;

    public ProductManagementService(IProductRepository productRepository, IUserServiceClient userServiceClient)
    {
        _productRepository = productRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, Guid userId)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null || product.IsDeleted)
            return null;

        if (product.UserId != userId)
            throw new UnauthorizedAccessException("Access denied to this product");

        return MapToDto(product);
    }

    public async Task<List<ProductDto>> GetUserProductsAsync(Guid userId)
    {
        var products = await _productRepository.GetByUserIdAsync(userId);
        return products.Where(p => !p.IsDeleted).Select(MapToDto).ToList();
    }

    public async Task<PagedResponse<ProductDto>> GetProductsByFilterAsync(ProductFilterDto filter, Guid userId)
    {
        filter.UserId = userId;

        var products = await _productRepository.GetByFilterAsync(filter);
        var filteredProducts = products.Where(p => !p.IsDeleted).ToList();

        var totalCount = filteredProducts.Count;
        var pagedProducts = filteredProducts
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResponse<ProductDto>
        {
            Items = pagedProducts,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, Guid userId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = createProductDto.Title,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            IsAvailable = createProductDto.IsAvailable,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto, Guid userId)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        if (product.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own products");
        }

        product.Title = updateProductDto.Title;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.IsAvailable = updateProductDto.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id, Guid userId)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        if (product.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own products");
        }

        await _productRepository.DeleteAsync(id);
    }

    public async Task UpdateProductsUserStatusAsync(Guid userId, bool isActive)
    {
        await _productRepository.SoftDeleteByUserIdAsync(userId, !isActive);
    }

    private ProductDto MapToDto(Product product) => new ProductDto
    {
        Id = product.Id,
        Title = product.Title,
        Description = product.Description,
        Price = product.Price,
        IsAvailable = product.IsAvailable,
        UserId = product.UserId,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}