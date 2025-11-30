using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Application.Models.DTOs;
using ProductService.Application.Models.Requests;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductManagementService _productService;

    public ProductsController(IProductManagementService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var userId = GetCurrentUserId();
        var result = await _productService.GetProductsByFilterAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var userId = GetCurrentUserId();
        var product = await _productService.GetProductByIdAsync(id, userId);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("my-products")]
    public async Task<IActionResult> GetMyProducts()
    {
        var userId = GetCurrentUserId();
        var products = await _productService.GetUserProductsAsync(userId);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        var userId = GetCurrentUserId();
        var product = await _productService.CreateProductAsync(createProductDto, userId);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        var userId = GetCurrentUserId();
        var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto, userId);
        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userId = GetCurrentUserId();
        await _productService.DeleteProductAsync(id, userId);
        return NoContent();
    }

    // нужен только для вызовов между сервисами
    [HttpPost("update-user-status")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateUserProductsStatus([FromBody] UpdateUserStatusRequest request)
    {
        await _productService.UpdateProductsUserStatusAsync(request.UserId, request.IsActive);
        return Ok();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}