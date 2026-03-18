using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.ViewModels;

public class EditOrderViewModel
{
    public int OrderId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Version { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string? Message { get; set; }

    public bool IsConflict { get; set; }

    public List<EditOrderItemViewModel> Items { get; set; } = new();

    public static EditOrderViewModel FromOrder(Order order)
    {
        return new EditOrderViewModel
        {
            OrderId = order.OrderId,
            StudentName = order.StudentName,
            Status = order.Status,
            Version = order.Version,
            Notes = order.Notes,
            Items = order.Items.Select(item => new EditOrderItemViewModel
            {
                MenuItemId = item.MenuItemId,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}
