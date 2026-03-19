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
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

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

    
    public string? Description { get; set; }

 
    public int StockQuantity { get; set; } = 100;

   
    public bool IsAvailable { get; set; } = true;

  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
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

  
    public string ReadProduct()
    {
        return $"{Name} - ?{Price:N0} (Stock: {StockQuantity})";
    }

 
    public bool CheckStock(int requestedQuantity)
    {
        return StockQuantity >= requestedQuantity && IsAvailable;
    }

   
    public bool DecrementStock(int quantity)
    {
        if (CheckStock(quantity))
        {
            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }


    public void IncrementStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }


    public string GetProductInfo()
    {
        return $"{Name} - ?{Price:N0}";
    }
}
