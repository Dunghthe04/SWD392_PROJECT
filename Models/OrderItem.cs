using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWD392_PROJECT.Models;

public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int MenuItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

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

    public static OrderItem CreateOrderItem(int targetOrderId, string targetItemName, int qty)
    {
        _ = targetOrderId;
        return new OrderItem
        {
            ItemName = targetItemName,
            Quantity = qty
        };
    }

    public string ReadOrderItem()
    {
        return $"Item: {ItemName}, Quantity: {Quantity}";
    }

    public static bool ValidateOrderItems(IEnumerable<OrderItem> items)
    {
        return items.All(item => !string.IsNullOrWhiteSpace(item.ItemName) && item.Quantity > 0);
    }

    public static bool ValidateOrderItems(int orderId, IEnumerable<OrderItem> items)
    {
        _ = orderId;
        return ValidateOrderItems(items);
    }

    public void UpdateOrderItem(int newQty)
    {
        if (newQty > 0)
        {
            Quantity = newQty;
        }
    }

    public void UpdateOrderItem(int newQty, string? newNote)
    {
        _ = newNote;
        UpdateOrderItem(newQty);
    }

    public void DeleteOrderItem()
    {
        Quantity = 0;
    }
}
