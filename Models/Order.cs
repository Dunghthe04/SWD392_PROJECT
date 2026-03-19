namespace SWD392_PROJECT.Models;

public class Order
{
    public int OrderId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateTime OrderTime { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = "Pending";
    
    public int? StallId { get; set; } 

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
            Items = new List<OrderItem>(targetItems) // Use items directly without cloning
        };

        // Only calculate total if items are provided
        if (targetItems.Count > 0)
        {
            order.RecalculateTotal();
        }

        return order;
    }

    public static Order CreateOrder(string targetStudentName, double targetTotalPrice)
    {
        var now = DateTime.UtcNow;
        return new Order
        {
            StudentName = targetStudentName,
            TotalPrice = Convert.ToDecimal(targetTotalPrice),
            OrderTime = now,
            Status = "Pending",
            CreatedAt = now,
            LastUpdatedAt = now
        };
    }

    public static Order CreateOrder(int targetStudentId, string initialNotes)
    {
        var now = DateTime.UtcNow;
        return new Order
        {
            StudentId = targetStudentId,
            Notes = initialNotes,
            Status = "Pending",
            OrderTime = now,
            CreatedAt = now,
            LastUpdatedAt = now,
            Version = 1
        };
    }

    public static List<Order> ReadOrderList(IEnumerable<Order> orders)
    {
        return orders.ToList();
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

    public void UpdateOrderStatus(string newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            return;
        }

        Status = newStatus;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public string UpdateOrder(string newNotes)
    {
        if (!string.IsNullOrWhiteSpace(newNotes))
        {
            Notes = newNotes;
        }

        LastUpdatedAt = DateTime.UtcNow;
        return "Updated";
    }

    public void DeleteOrder()
    {
        Status = "Cancelled";
        LastUpdatedAt = DateTime.UtcNow;
    }

    public int GetStudentId()
    {
        return StudentId;
    }

    public bool IsUpdatable()
    {
        return CheckUpdatableStatus();
    }

    public bool CheckUpdatableStatus()
    {
        var isCompleted = string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase);
        var isCancelled = string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase);
        return !isCompleted && !isCancelled && !IsLocked;
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

    public void RecalculateTotal()
    {
        TotalPrice = Items.Sum(item => item.LineTotal);
    }
}