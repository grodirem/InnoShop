using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.API.Controllers;
using ProductService.Application.Interfaces;
using ProductService.Application.Models.DTOs;
using System.Security.Claims;

namespace ProductService.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductManagementService> _productServiceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductManagementService>();
        _controller = new ProductsController(_productServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        ], "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetProduct_WhenProductExists_ReturnsOk()
    {
        var productId = Guid.NewGuid();
        var productDto = new ProductDto { Id = productId, Title = "Test Product" };

        _productServiceMock
            .Setup(service => service.GetProductByIdAsync(productId, It.IsAny<Guid>()))
            .ReturnsAsync(productDto);

        var result = await _controller.GetProduct(productId);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var productId = Guid.NewGuid();

        _productServiceMock
            .Setup(service => service.GetProductByIdAsync(productId, It.IsAny<Guid>()))
            .ReturnsAsync((ProductDto)null);

        var result = await _controller.GetProduct(productId);

        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        var createProductDto = new CreateProductDto
        {
            Title = "New Product",
            Description = "Description",
            Price = 100.50m,
            IsAvailable = true
        };

        var createdProduct = new ProductDto
        {
            Id = Guid.NewGuid(),
            Title = "New Product"
        };

        _productServiceMock
            .Setup(service => service.CreateProductAsync(createProductDto, It.IsAny<Guid>()))
            .ReturnsAsync(createdProduct);

        var result = await _controller.CreateProduct(createProductDto);

        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be("GetProduct");
    }

    [Fact]
    public async Task GetMyProducts_ReturnsUsersProducts()
    {
        var products = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Title = "Product 1" },
            new ProductDto { Id = Guid.NewGuid(), Title = "Product 2" }
        };

        _productServiceMock
            .Setup(service => service.GetUserProductsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(products);

        var result = await _controller.GetMyProducts();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedProducts = okResult.Value as List<ProductDto>;
        returnedProducts.Should().HaveCount(2);
    }
}