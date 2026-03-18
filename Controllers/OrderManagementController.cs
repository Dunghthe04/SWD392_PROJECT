using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Coordinators;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using SWD392_PROJECT.ViewModels;

namespace SWD392_PROJECT.Controllers;

public class OrderManagementController : Controller
{
    private readonly ViewOrderCoordinator _viewOrderCoordinator;
    private readonly UpdateOrderCoordinator _updateOrderCoordinator;
    private readonly IOrderService _orderService;
    private readonly IStaffContext _staffContext;

    public OrderManagementController(
        ViewOrderCoordinator viewOrderCoordinator,
        UpdateOrderCoordinator updateOrderCoordinator,
        IOrderService orderService,
        IStaffContext staffContext)
    {
        _viewOrderCoordinator = viewOrderCoordinator;
        _updateOrderCoordinator = updateOrderCoordinator;
        _orderService = orderService;
        _staffContext = staffContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!_staffContext.IsAuthenticated || !_staffContext.HasOrderManagementPermission)
        {
            return Forbid();
        }

        var result = _viewOrderCoordinator.RetrieveOrderList();

        var model = new OrderListViewModel
        {
            Orders = result.OrderList,
            Message = TempData["OrderMessage"]?.ToString() ?? result.Message,
            IsError = result.IsError
        };

        return View(model);
    }

    [HttpGet]
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

        var updatedItems = model.Items.Select(item => new OrderItem
        {
            MenuItemId = item.MenuItemId,
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();

        var result = _updateOrderCoordinator.ProcessUpdateOrder(
            model.OrderId,
            _staffContext.StaffId,
            updatedItems,
            model.Notes,
            model.Version);

        if (result.Success)
        {
            TempData["OrderMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var fallbackOrder = result.CurrentOrder ?? _orderService.FindOrder(model.OrderId);
        if (fallbackOrder is null)
        {
            TempData["OrderMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var editModel = EditOrderViewModel.FromOrder(fallbackOrder);
        editModel.Message = result.Message;
        editModel.IsConflict = result.IsConflict;

        return View(editModel);
    }
}
