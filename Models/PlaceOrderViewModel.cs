namespace SWD392_PROJECT.Models;

public class PlaceOrderViewModel
{
    public List<CartItemRequest> CartItems { get; set; } = new();
    public string PaymentMethod { get; set; } = "Cash";
    public string Notes { get; set; } = "";
}

public class CartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
