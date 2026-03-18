using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.ViewModels;

public class OrderListViewModel
{
    public List<Order> Orders { get; set; } = new();

    public string? Message { get; set; }

    public bool IsError { get; set; }
}
