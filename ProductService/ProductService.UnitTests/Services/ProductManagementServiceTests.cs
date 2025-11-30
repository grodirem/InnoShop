using FluentAssertions;
using Moq;
using ProductService.Application.Interfaces;
using ProductService.Application.Models.DTOs;
using ProductService.Application.Services;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Services;

public class ProductManagementServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUserServiceClient> _userServiceClientMock;
    private readonly ProductManagementService _productService;

    public ProductManagementServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _userServiceClientMock = new Mock<IUserServiceClient>();
        _productService = new ProductManagementService(
            _productRepositoryMock.Object,
            _userServiceClientMock.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsProduct()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            UserId = userId
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var result = await _productService.GetProductByIdAsync(productId, userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Title.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        var result = await _productService.GetProductByIdAsync(productId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenUserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        var productId = Guid.NewGuid();
        var productOwnerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            UserId = productOwnerId
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _productService.GetProductByIdAsync(productId, differentUserId));
    }

    [Fact]
    public async Task UpdateProductAsync_WhenUserIsOwner_UpdatesProduct()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateProductDto = new UpdateProductDto
        {
            Title = "Updated Product",
            Description = "Updated Description",
            Price = 150.75m,
            IsAvailable = false
        };

        var existingProduct = new Product
        {
            Id = productId,
            Title = "Original Product",
            Description = "Original Description",
            Price = 100.50m,
            IsAvailable = true,
            UserId = userId
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        var result = await _productService.UpdateProductAsync(productId, updateProductDto, userId);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Product");
        result.Description.Should().Be("Updated Description");
        result.Price.Should().Be(150.75m);
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProductAsync_WhenUserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        var productId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var updateProductDto = new UpdateProductDto
        {
            Title = "Updated Product"
        };

        var existingProduct = new Product
        {
            Id = productId,
            Title = "Original Product",
            UserId = ownerId
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _productService.UpdateProductAsync(productId, updateProductDto, differentUserId));
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductExists_DeletesProduct()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Title = "Product to Delete",
            UserId = userId
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(repo => repo.DeleteAsync(productId))
            .Returns(Task.CompletedTask);

        await _productService.DeleteProductAsync(productId, userId);

        _productRepositoryMock.Verify(repo => repo.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetUserProductsAsync_ReturnsOnlyUsersProducts()
    {
        var userId = Guid.NewGuid();
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Title = "Product 1", UserId = userId },
            new Product { Id = Guid.NewGuid(), Title = "Product 2", UserId = userId },
            new Product { Id = Guid.NewGuid(), Title = "Other User Product", UserId = Guid.NewGuid() }
        };

        _productRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(products.Where(p => p.UserId == userId).ToList());

        var result = await _productService.GetUserProductsAsync(userId);

        result.Should().HaveCount(2);
        result.All(p => p.UserId == userId).Should().BeTrue();
    }
}