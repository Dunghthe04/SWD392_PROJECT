using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Controllers;

[Authorize(Roles = "Student,CanteenStaff,Manager")]
[Route("[controller]")]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IStaffContext _staffContext;
    private readonly IPermissionService _permissionService;
    private readonly IValidationService _validationService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public OrderController(
        IOrderService orderService,
        IStaffContext staffContext,
        IPermissionService permissionService,
        IValidationService validationService,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _orderService = orderService;
        _staffContext = staffContext;
        _permissionService = permissionService;
        _validationService = validationService;
        _auditService = auditService;
        _notificationService = notificationService;
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
            TempData["OrderMessage"] = "This order cannot be updated because it is already processed, cancelled, or locked.";
            return RedirectToAction(nameof(Index));
        }

        return View(EditOrderViewModel.FromOrder(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Edit")]
    public IActionResult Edit(EditOrderViewModel model, string submitAction)
    {
        if (!_staffContext.IsAuthenticated || !_staffContext.HasOrderManagementPermission)
        {
            return Forbid();
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

        if (!order.IsUpdatable())
        {
            var notUpdatableModel = EditOrderViewModel.FromOrder(order);
            notUpdatableModel.Message = "This order cannot be updated because it is already processed, cancelled, or locked.";
            return View(notUpdatableModel);
        }

        // Build updated items
        var updatedItems = model.Items.Select(item => new OrderItem
        {
            MenuItemId = item.MenuItemId,
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            UnitPrice = Double.Parse(item.UnitPrice.ToString())
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

        order.UpdateDetails(updatedItems, model.Notes);

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
}
