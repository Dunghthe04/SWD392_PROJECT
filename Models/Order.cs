namespace SWD392_PROJECT.Models;

public class Order
{
    public int OrderId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateTime OrderTime { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = "Pending";

    public string Notes { get; set; } = string.Empty;

    public bool IsLocked { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public static Order CreateOrder(int orderId, int studentId, string studentName, List<OrderItem> targetItems, string notes)
    {
        var now = DateTime.UtcNow;
        var order = new Order
        {
            OrderId = orderId,
            StudentId = studentId,
            StudentName = studentName,
            OrderTime = now,
            Status = "Pending",
            Notes = notes,
            IsLocked = false,
            Version = 1,
            CreatedAt = now,
            LastUpdatedAt = now,
            Items = targetItems.Select(item => item.Clone()).ToList()
        };

        order.RecalculateTotal();
        return order;
    }

    public string ReadOrder()
    {
        return $"Order ID: {OrderId}, Student Name: {StudentName}, Order Time: {OrderTime:O}, Total Price: {TotalPrice:0.00}, Status: {Status}";
    }

    public void UpdateDetails(List<OrderItem> newItems, string newNotes)
    {
        Notes = newNotes;
        Items = newItems.Select(item => item.Clone()).ToList();
        LastUpdatedAt = DateTime.UtcNow;
        RecalculateTotal();
    }

    public int GetStudentId()
    {
        return StudentId;
    }

    public bool IsUpdatable()
    {
        return Status != "Processing"
            && Status != "Completed"
            && Status != "Cancelled"
            && !IsLocked;
    }

    public Order Clone()
    {
        return new Order
        {
            OrderId = OrderId,
            StudentId = StudentId,
            StudentName = StudentName,
            OrderTime = OrderTime,
            TotalPrice = TotalPrice,
            Status = Status,
            Notes = Notes,
            IsLocked = IsLocked,
            Version = Version,
            CreatedAt = CreatedAt,
            LastUpdatedAt = LastUpdatedAt,
            Items = Items.Select(item => item.Clone()).ToList()
        };
    }

    private void RecalculateTotal()
    {
        TotalPrice = (decimal)Items.Sum(item => item.LineTotal);
    }
}
