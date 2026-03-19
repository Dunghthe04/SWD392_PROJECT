using SWD392_PROJECT.Models;
using SWD392_PROJECT.Repositories.Interfaces;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

/// <summary>
/// Service for Product business logic
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Get all available products
    /// </summary>
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductService] Getting all products");
            var products = await _productRepository.GetAllProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[ProductService] Retrieved {products.Count()} products");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Exception in GetAllProductsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Inner exception: {ex.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Getting products for category: {category}");
            var products = await _productRepository.GetProductsByCategoryAsync(category);
            System.Diagnostics.Debug.WriteLine($"[ProductService] Retrieved {products.Count()} products for category {category}");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Exception in GetProductsByCategoryAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _productRepository.GetProductByIdAsync(productId);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductService] Getting all categories");
            var categories = await _productRepository.GetCategoriesAsync();
            System.Diagnostics.Debug.WriteLine($"[ProductService] Retrieved {categories.Count()} categories");
            return categories;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Exception in GetCategoriesAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Search products
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllProductsAsync();

        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Searching products for: {searchTerm}");
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            System.Diagnostics.Debug.WriteLine($"[ProductService] Found {products.Count()} products for search: {searchTerm}");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Exception in SearchProductsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
