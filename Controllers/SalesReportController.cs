using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Controllers;

// CLASS 1: SalesReportCoordinator («control»)
public class SalesReportController : Controller
{
    private readonly AppDbContext _context;

    public SalesReportController(AppDbContext context)
    {
        _context = context;
    }

    // M1, M2: viewSalesReport() -> requestSalesReport()
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stalls = await _context.Stalls.ToListAsync(); // M3: getStallList() -> M4
        
        var model = new SalesReportViewModel
        {
            AvailableStalls = stalls,
            SelectedTimeRange = "Weekly" 
        };

        return View(model); // M5, M6: showFilters
    }

    // M7, M8: submitReportCriteria / processReportRequest
    // M16a, M17a: changeTimeRange / updateCriteria
    [HttpPost]
    public async Task<IActionResult> Index(SalesReportViewModel input, string actionType = "generate")
    {
        input.AvailableStalls = await _context.Stalls.ToListAsync();

        // E2: Invalid Time Range
        if (input.SelectedTimeRange != "Daily" && input.SelectedTimeRange != "Weekly" && input.SelectedTimeRange != "Monthly")
        {
            input.ErrorMessage = "Error: Invalid time range. Please select 'Daily', 'Weekly', or 'Monthly'.";
            return View(input);
        }

        // Parse TimeRange to Exact Dates to match UML methods
        DateTime endDate = DateTime.UtcNow;
        DateTime startDate = endDate;
        if (input.SelectedTimeRange == "Daily") startDate = endDate.Date;
        else if (input.SelectedTimeRange == "Weekly") startDate = endDate.AddDays(-7);
        else if (input.SelectedTimeRange == "Monthly") startDate = endDate.AddMonths(-1);

        // A2: Nếu do sự kiện Auto-submit khi đổi Dropdown, gọi hàm RefreshSalesReport
        if (actionType == "refresh")
        {
            return await RefreshSalesReport(startDate, endDate, input.SelectedStallId, input);
        }

        // Truyền thống: Bấm nút View Report thì gọi GenerateSalesReport
        return await GenerateSalesReport(startDate, endDate, input.SelectedStallId, input);
    }

    // UML Operation: +refreshSalesReport(startDate : Date, endDate : Date, stallId : int)
    public async Task<IActionResult> RefreshSalesReport(DateTime startDate, DateTime endDate, int? stallId, SalesReportViewModel input)
    {
        return await GenerateSalesReport(startDate, endDate, stallId, input);
    }

    // UML Operation: +generateSalesReport(startDate : Date, endDate : Date, stallId : int)
    public async Task<IActionResult> GenerateSalesReport(DateTime startDate, DateTime endDate, int? stallId, SalesReportViewModel input)
    {
        // M9: Order.getCompletedOrders(timeRange, stallID)
        var ordersQuery = _context.Orders.Where(o => o.Status == "Completed" || o.Status == "Success");
        ordersQuery = ordersQuery.Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate);
        
        // A1: Filter by Stall
        if (stallId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.StallId == stallId.Value);
        }

        var orders = await ordersQuery.ToListAsync(); // M10: returnOrders()

        // E1: No Sales Data
        if (!orders.Any())
        {
            input.ErrorMessage = "No sales data is available for the selected criteria.";
            return View("Index", input); // M14a, M15a
        }

        // M11: Payment.getPaymentData(orderIds)
        var orderIds = orders.Select(o => o.OrderId).ToList();
        var payments = await _context.Payments
            .Where(p => orderIds.Contains(p.OrderId) && p.Status == "Success")
            .ToListAsync(); // M12: returnPaymentData()
        
        // === IMPLEMENT PSEUDOCODE FOR DATA ABSTRACTION ===
        var reportCalc = new SalesReport();
        var revenue = reportCalc.CalculateRevenue(payments);
        var count = reportCalc.CalculateOrderCount(orders);

        var report = new SalesReport
        {
            StartDate = startDate,
            EndDate = endDate,
            StallId = stallId,
            TotalRevenue = revenue,
            CompletedOrders = count
        };

        input.ReportSummary = report;

        // UI List Details mapping
        var stallsDict = input.AvailableStalls.ToDictionary(s => s.StallId, s => s.Name);
        var details = new List<SalesReportItem>();
        foreach(var order in orders)
        {
            var pAmount = payments.Where(p => p.OrderId == order.OrderId).Sum(p => p.Amount);
            var stallName = order.StallId.HasValue && stallsDict.ContainsKey(order.StallId.Value) 
                            ? stallsDict[order.StallId.Value] : "Unknown";

            details.Add(new SalesReportItem
            {
                OrderId = order.OrderId,
                StallName = stallName,
                OrderDate = order.OrderTime,
                Amount = pAmount,
                Status = order.Status
            });
        }
        input.ReportDetails = details;

        // M13, M14: sendReportData() / displaySalesReport()
        return View("Index", input); 
    }

    [HttpPost]
    public async Task<IActionResult> Export(SalesReportViewModel input)
    {
        DateTime endDate = DateTime.UtcNow;
        DateTime startDate = endDate;
        if (input.SelectedTimeRange == "Daily") startDate = endDate.Date;
        else if (input.SelectedTimeRange == "Weekly") startDate = endDate.AddDays(-7);
        else if (input.SelectedTimeRange == "Monthly") startDate = endDate.AddMonths(-1);

        var ordersQuery = _context.Orders.Where(o => o.Status == "Completed" || o.Status == "Success");
        ordersQuery = ordersQuery.Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate);
        if (input.SelectedStallId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.StallId == input.SelectedStallId.Value);
        }

        var orders = await ordersQuery.ToListAsync();
        if (!orders.Any()) return BadRequest("No data to export");

        var orderIds = orders.Select(o => o.OrderId).ToList();
        var payments = await _context.Payments
            .Where(p => orderIds.Contains(p.OrderId) && p.Status == "Success")
            .ToListAsync();
            
        var stallsDict = await _context.Stalls.ToDictionaryAsync(s => s.StallId, s => s.Name);
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Order ID,Stall Name,Date,Amount,Status");
        
        foreach(var order in orders)
        {
            var amt = payments.Where(p => p.OrderId == order.OrderId).Sum(p => p.Amount);
            var stallName = order.StallId.HasValue && stallsDict.ContainsKey(order.StallId.Value) 
                            ? stallsDict[order.StallId.Value] : "Unknown";
            builder.AppendLine($"{order.OrderId},\"{stallName}\",\"{order.OrderTime:yyyy-MM-dd HH:mm}\",{amt},{order.Status}");
        }

        var buffer = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var finalBuffer = new byte[bom.Length + buffer.Length];
        System.Buffer.BlockCopy(bom, 0, finalBuffer, 0, bom.Length);
        System.Buffer.BlockCopy(buffer, 0, finalBuffer, bom.Length, buffer.Length);

        string fileName = $"SalesReport_{input.SelectedTimeRange}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(finalBuffer, "text/csv", fileName);
    }
}
