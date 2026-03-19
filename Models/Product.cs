namespace SWD392_PROJECT.Models;

/// <summary>
/// Represents a food product/menu item in the Campus Food system
/// </summary>
public class Product
{
    /// <summary>
    /// Primary key - unique product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price (changed from decimal to float)
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Product category (e.g., "Breakfast", "Lunch", "Snacks", "Drinks")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Product selling/availability time (e.g., "7:00 AM - 11:00 AM")
    /// </summary>
    public string SellingTime { get; set; } = string.Empty;

    /// <summary>
    /// Product image URL/path
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is product available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Product creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    // UML Method: +setProductInfo(productData)
    public void SetProductInfo(CreateProductViewModel productData)
    {
        Name = productData.Name;
        Price = productData.Price;
        Category = productData.Category;
        SellingTime = productData.SellingTime;

        if (productData.Quantity > 0)
            IsAvailable = true;
        else
            IsAvailable = false;
    }

    // UML Method: +createProduct()
    public void CreateProduct()
    {
        // Hàm đại diện theo yêu cầu UML Pseudocode. 
        // Về bản chất EF Core sẽ lưu bằng lệnh Add() ở DBContext.
    }
}
