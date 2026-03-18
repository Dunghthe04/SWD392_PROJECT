using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Coordinators;

public class UpdateOrderResult
{
    public bool Success { get; init; }

    public bool IsConflict { get; init; }

    public string Message { get; init; } = string.Empty;

    public Order? CurrentOrder { get; init; }
}
