namespace SWD392_PROJECT.Models;

// Entity Object: StallProduct
public class StallProduct
{
    public int StallProductId { get; set; }
    public int ProductId { get; set; }
    public int StallId { get; set; }
    public int Quantity { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public Stall Stall { get; set; } = null!;

    // UML Message M11: createStock(productId, stallId, quantity)
    public bool CreateStock(int productId, int stallId, int quantity)
    {
        ProductId = productId;
        StallId = stallId;
        Quantity = quantity;
        
        return true; // M12: stockCreated trả về
    }
}
