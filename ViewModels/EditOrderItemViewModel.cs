namespace SWD392_PROJECT.ViewModels;

public class EditOrderItemViewModel
{
    public int MenuItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
