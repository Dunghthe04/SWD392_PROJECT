namespace SWD392_PROJECT.Models;

// CLASS 2: SalesReport <<data abstraction class>> (Chuẩn hóa theo UML)
public class SalesReport
{
    public int ReportId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? StallId { get; set; }
    public double TotalRevenue { get; set; }
    public int CompletedOrders { get; set; }

    // +calculateRevenue(paymentList)
    public double CalculateRevenue(List<Payment> paymentList)
    {
        double total = 0;
        foreach (var payment in paymentList)
        {
            total += (double)payment.Amount;
        }
        return total;
    }

    // +calculateOrderCount(orderList)
    public int CalculateOrderCount(List<Order> orderList)
    {
        int count = 0;
        foreach (var order in orderList)
        {
            count += 1;
        }
        return count;
    }
}

// Bổ trợ hiển thị ở View
public class SalesReportItem
{
    public int OrderId { get; set; }
    public string StallName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ViewModel bọc lại để truyền xuống View (cầu nối MVC)
public class SalesReportViewModel
{
    // Filter Options
    public List<Stall> AvailableStalls { get; set; } = new();
    public string SelectedTimeRange { get; set; } = "Weekly";
    public int? SelectedStallId { get; set; }
    
    // Result Data chứa bản sao của Data Abstraction Class
    public SalesReport? ReportSummary { get; set; }
    public List<SalesReportItem> ReportDetails { get; set; } = new();
    
    // Thuộc tính để UI không bị vỡ (Backward Map)
    public int TotalOrders => ReportSummary?.CompletedOrders ?? 0;
    public decimal TotalRevenue => (decimal)(ReportSummary?.TotalRevenue ?? 0);
    
    // Error Handling (E1, E2)
    public string? ErrorMessage { get; set; }
}