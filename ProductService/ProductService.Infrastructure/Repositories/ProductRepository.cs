using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Application.Models.DTOs;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<List<Product>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByFilterAsync(ProductFilterDto filter)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(p =>
                p.Title.Contains(filter.Search) ||
                p.Description.Contains(filter.Search));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (filter.IsAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == filter.IsAvailable.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(p => p.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "price" => filter.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "title" => filter.SortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
                "createdat" => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteByUserIdAsync(Guid userId, bool isDeleted)
    {
        var products = await _context.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsDeleted = isDeleted;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}