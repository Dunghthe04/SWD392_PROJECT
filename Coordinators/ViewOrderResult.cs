using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Coordinators;

public class ViewOrderResult
{
    public List<Order> OrderList { get; init; } = new();

    public string? Message { get; init; }

    public bool IsError { get; init; }
}
