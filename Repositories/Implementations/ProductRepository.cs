using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Repositories.Implementations;

/// <summary>
/// Repository for Product entity operations
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all available products
    /// </summary>
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return products;
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => p.Category == category && p.IsAvailable)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Products
            .Where(p => p.IsAvailable)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    /// <summary>
    /// Search products by name
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _context.Products
            .Where(p => p.IsAvailable && (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Add new product
    /// </summary>
    public async Task<Product> AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Update product
    /// </summary>
    public async Task<Product> UpdateProductAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Delete product
    /// </summary>
    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
