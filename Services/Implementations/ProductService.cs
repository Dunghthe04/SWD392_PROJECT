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
        return await _productRepository.GetAllProductsAsync();
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _productRepository.GetProductsByCategoryAsync(category);
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
        return await _productRepository.GetCategoriesAsync();
    }

    /// <summary>
    /// Search products
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllProductsAsync();

        return await _productRepository.SearchProductsAsync(searchTerm);
    }
}
