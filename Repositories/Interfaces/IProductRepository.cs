using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Repositories.Interfaces;

/// <summary>
/// Interface for Product repository operations
/// </summary>
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<Product?> GetProductByIdAsync(int productId);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task<Product> AddProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int productId);
}
