using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using SWD392_PROJECT.Repositories.Interfaces;
using SWD392_PROJECT.Data.Repositories.Interfaces;

namespace SWD392_PROJECT.Controllers;

[Authorize(Roles = "Student,CanteenStaff,Manager")]
[Route("[controller]")]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IStaffContext _staffContext;
    private readonly IPermissionService _permissionService;
    private readonly IValidationService _validationService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderController(
        IOrderService orderService,
        IProductService productService,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IStaffContext staffContext,
        IPermissionService permissionService,
        IValidationService validationService,
        IAuditService auditService,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderService = orderService;
        _productService = productService;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _staffContext = staffContext;
        _permissionService = permissionService;
        _validationService = validationService;
        _auditService = auditService;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public IActionResult Index()
    {
        if (!_staffContext.IsAuthenticated || !_staffContext.HasOrderManagementPermission)
        {
            return Forbid();
        }

        var orderList = _orderService.ReadOrderList();

        var model = new OrderListViewModel();

        if (orderList is null)
        {
            model.IsError = true;
            model.Message = TempData["OrderMessage"]?.ToString() ?? "Unable to retrieve the order list.";
        }
        else if (orderList.Count == 0)
        {
            model.Message = TempData["OrderMessage"]?.ToString() ?? "No orders are available.";
        }
        else
        {
            model.Orders = orderList;
            model.Message = TempData["OrderMessage"]?.ToString();
        }

        return View(model);
    }

    [HttpGet]
    [Route("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        if (!_staffContext.IsAuthenticated || !_staffContext.HasOrderManagementPermission)
        {
            return Forbid();
        }

        var order = _orderService.FindOrder(id);
        if (order is null)
        {
            TempData["OrderMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!order.IsUpdatable())
        {
            TempData["OrderMessage"] = "This order cannot be updated when status is Completed/Cancelled or when it is locked.";
            return RedirectToAction(nameof(Index));
        }

        return View(EditOrderViewModel.FromOrder(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Edit")]
    [Route("Edit/{id?}")]
    public IActionResult Edit(EditOrderViewModel model, string submitAction, int? id = null)
    {
        if (!_staffContext.IsAuthenticated || !_staffContext.HasOrderManagementPermission)
        {
            return Forbid();
        }

        if (id.HasValue && model.OrderId == 0)
        {
            model.OrderId = id.Value;
        }

        if (string.Equals(submitAction, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            TempData["OrderMessage"] = "Update cancelled. No changes were saved.";
            return RedirectToAction(nameof(Index));
        }

        // Check permission
        var accessGranted = _permissionService.CanUpdateOrder(_staffContext.StaffId, model.OrderId);
        if (!accessGranted)
        {
            TempData["OrderMessage"] = "You do not have permission to update this order.";
            return RedirectToAction(nameof(Index));
        }

        var order = _orderService.FindOrder(model.OrderId);
        if (order is null)
        {
            TempData["OrderMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        var allowedStatuses = new[] { "Pending", "Processing", "Completed", "Cancelled" };
        if (!allowedStatuses.Contains(model.Status))
        {
            var invalidStatusModel = EditOrderViewModel.FromOrder(order);
            invalidStatusModel.Message = "Invalid order status.";
            return View(invalidStatusModel);
        }

        if (!order.IsUpdatable())
        {
            var notUpdatableModel = EditOrderViewModel.FromOrder(order);
            notUpdatableModel.Message = "This order cannot be updated when status is Completed/Cancelled or when it is locked.";
            return View(notUpdatableModel);
        }

        // Build updated items
        var updatedItems = model.Items.Select(item => new OrderItem
        {
            MenuItemId = item.MenuItemId,
            ItemName = item.ItemName,
            Quantity = item.Quantity,

            UnitPrice = (double)item.UnitPrice

        }).ToList();

        // Validate
        var validationPassed = _validationService.Validate(updatedItems, model.Notes, out var validationError);
        if (!validationPassed)
        {
            var invalidModel = EditOrderViewModel.FromOrder(order);
            invalidModel.Message = validationError;
            return View(invalidModel);
        }

        // Optimistic concurrency check
        var saveAllowed = _orderService.CheckVersion(model.OrderId, model.Version);
        if (!saveAllowed)
        {
            var latestOrder = _orderService.FindOrder(model.OrderId);
            var conflictModel = latestOrder is not null
                ? EditOrderViewModel.FromOrder(latestOrder)
                : new EditOrderViewModel { OrderId = model.OrderId };
            conflictModel.Message = "Update conflict detected. The order was changed by another process. Please review the latest data and try again.";
            conflictModel.IsConflict = true;
            return View(conflictModel);
        }

        order.Notes = model.Notes;
        order.Status = model.Status;
        foreach (var updatedItem in updatedItems)
        {
            var existingItem = order.Items.FirstOrDefault(i => i.MenuItemId == updatedItem.MenuItemId);
            if (existingItem is null)
            {
                continue;
            }

            existingItem.ItemName = updatedItem.ItemName;
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.UnitPrice = updatedItem.UnitPrice;
        }

        var saved = _orderService.Save(order, model.Version);
        if (!saved)
        {
            var latestOrder = _orderService.FindOrder(model.OrderId);
            var conflictModel = latestOrder is not null
                ? EditOrderViewModel.FromOrder(latestOrder)
                : new EditOrderViewModel { OrderId = model.OrderId };
            conflictModel.Message = "Update conflict detected. Please reload the order and try again.";
            conflictModel.IsConflict = true;
            return View(conflictModel);
        }

        _auditService.Log("Update Order", model.OrderId, _staffContext.StaffId);
        var studentId = order.GetStudentId();
        _notificationService.NotifyOrderUpdated(studentId, model.OrderId);

        TempData["OrderMessage"] = "Order updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================
    // PLACE ORDER - New Feature
    // ============================================

    /// <summary>
    /// M10: Display checkout page - Get order details before confirmation
    /// </summary>
    [HttpGet]
    [Route("Checkout")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            var model = new PlaceOrderViewModel();
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToAction("Browse", "Menu");
        }
    }

    /// <summary>
    /// M11-M12: Get order summary (check stock availability)
    /// </summary>
    [HttpPost]
    [Route("GetOrderSummary")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetOrderSummary([FromBody] List<CartItemRequest> cartItems)
    {
        try
        {
            if (cartItems == null || cartItems.Count == 0)
            {
                return Json(new { success = false, message = "Cart is empty" });
            }

            var orderItems = new List<OrderItem>();
            double totalPrice = 0;
            var errors = new List<string>();

            // M11-M12: Check stock for each item
            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    errors.Add($"Product {item.ProductId} not found");
                    continue;
                }

                // Check stock availability
                if (!product.CheckStock(item.Quantity))
                {
                    errors.Add($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
                    continue;
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = product.ProductId,
                    ItemName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                orderItems.Add(orderItem);
                totalPrice += orderItem.LineTotal;
            }

            if (errors.Any())
            {
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            return Json(new 
            { 
                success = true, 
                message = "Order summary calculated",
                orderItems = orderItems.Select(x => new 
                { 
                    x.MenuItemId,
                    x.ItemName, 
                    x.Quantity, 
                    x.UnitPrice, 
                    subTotal = x.LineTotal 
                }),
                totalPrice = totalPrice,
                itemCount = orderItems.Count
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// M13-M17: Place order (Create order, create items, decrement stock, process payment)
    /// </summary>
    [HttpPost]
    [Route("PlaceOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderViewModel model)
    {
        try
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Model received: {model}");
            System.Diagnostics.Debug.WriteLine($"[PlaceOrder] CartItems count: {model?.CartItems?.Count ?? -1}");
            if (model?.CartItems != null)
            {
                foreach (var item in model.CartItems)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Item: ProductId={item.ProductId}, Quantity={item.Quantity}");
                }
            }

            // Get current user
            var userId = User.FindFirst("UserId")?.Value ?? "0";
            var studentName = User.FindFirst("Name")?.Value ?? "Student";

            if (!int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            if (model?.CartItems == null || model.CartItems.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[PlaceOrder] Cart is empty or null");
                return Json(new { success = false, message = "Cart is empty" });
            }

            // Step 1 (M11-M12): Validate stock
            var products = new Dictionary<int, Product>();
            foreach (var item in model.CartItems)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null || !product.CheckStock(item.Quantity))
                {
                    return Json(new { success = false, message = $"Stock validation failed for product {item.ProductId}" });
                }
                products[product.ProductId] = product;
            }

            // Step 2 (M13): Create empty order first to get OrderId
            var order = Order.CreateOrder(
                orderId: 0,
                studentId: userIdInt,
                studentName: studentName,
                targetItems: new List<OrderItem>(),
                notes: model.Notes ?? ""
            );

            System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Order created in memory (empty)");

            // Step 3 (M14a): Save order first to get OrderId
            try
            {
                _orderRepository.CreateOrder(order);
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Order saved successfully. OrderId={order.OrderId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Error saving order: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Inner exception: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Failed to create order: {ex.InnerException?.Message ?? ex.Message}" });
            }

            // Step 4 (M14b): Calculate total price
            decimal totalPrice = 0;
            try
            {
                foreach (var item in model.CartItems)
                {
                    var product = products[item.ProductId];
                    totalPrice += (decimal)(item.Quantity * product.Price);
                    System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Item: {product.Name}, Qty={item.Quantity}, Price={product.Price}");
                }

                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Total Price Calculated: {totalPrice}");

                // Update order with total price and set to Paid status using new method
                _orderRepository.UpdateOrderTotal(order.OrderId, totalPrice, "Paid");
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Order updated with TotalPrice={totalPrice}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Error updating order total: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PlaceOrder] Inner exception: {ex.InnerException?.Message}");
                return Json(new { success = false, message = $"Failed to update order: {ex.InnerException?.Message ?? ex.Message}" });
            }

            // Step 5 (M15): Decrement stock
            foreach (var item in model.CartItems)
            {
                var product = products[item.ProductId];
                if (!product.DecrementStock(item.Quantity))
                {
                    return Json(new { success = false, message = "Failed to update stock" });
                }
                await _productRepository.UpdateProductAsync(product);
            }

            // Success response (M18)
            return Json(new 
            { 
                success = true, 
                message = "Order placed successfully!",
                orderId = order.OrderId,
                totalPrice = totalPrice
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}
