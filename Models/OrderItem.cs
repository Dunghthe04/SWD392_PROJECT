namespace SWD392_PROJECT.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int MenuItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public double UnitPrice { get; set; }

    public double LineTotal => Quantity * UnitPrice;

    public OrderItem Clone()
    {
        return new OrderItem
        {
            
            OrderId = OrderId,
            MenuItemId = MenuItemId,
            ItemName = ItemName,
            Quantity = Quantity,
            UnitPrice = UnitPrice
        };
    }
}
