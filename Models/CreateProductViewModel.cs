using System.ComponentModel.DataAnnotations;

namespace SWD392_PROJECT.Models;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "Product name is required")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selling time is required")]
    public string SellingTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a stall for initial stock")]
    public int StallId { get; set; }

    // Dropdown data
    public List<Stall> AvailableStalls { get; set; } = new();
    
    // Status message for UI
    public string? StatusMessage { get; set; }
    public bool IsSuccess { get; set; }
}
