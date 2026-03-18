namespace SWD392_PROJECT.Models;

public class OrderItem
{
    public int MenuItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public OrderItem Clone()
    {
        return new OrderItem
        {
            MenuItemId = MenuItemId,
            ItemName = ItemName,
            Quantity = Quantity,
            UnitPrice = UnitPrice
        };
    }
}
