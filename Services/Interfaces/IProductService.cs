using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services.Interfaces;

/// <summary>
/// Interface for Product service
/// </summary>
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<Product?> GetProductByIdAsync(int productId);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
}
