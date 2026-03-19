using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class UpdateOrderCoordinator : IUpdateOrderCoordinator
{
    private readonly IOrderService _orderService;
    private readonly IPermissionService _permissionService;
    private readonly IValidationService _validationService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public UpdateOrderCoordinator(
        IOrderService orderService,
        IPermissionService permissionService,
        IValidationService validationService,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _orderService = orderService;
        _permissionService = permissionService;
        _validationService = validationService;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public ViewOrderResult RetrieveOrderList()
    {
        var orderList = _orderService.ReadOrderList();
        if (orderList is null)
        {
            return new ViewOrderResult
            {
                IsError = true,
                Message = "Unable to retrieve the order list."
            };
        }

        return new ViewOrderResult
        {
            OrderList = orderList
        };
    }

    public UpdateOrderResult RetrieveOrderInformation(int orderId)
    {
        var order = _orderService.FindOrder(orderId);
        if (order is null)
        {
            return new UpdateOrderResult
            {
                Success = false,
                Message = "Order not found."
            };
        }

        if (!CheckOrderStatus(order))
        {
            return new UpdateOrderResult
            {
                Success = false,
                CurrentOrder = order,
                Message = "The order cannot be updated."
            };
        }

        return new UpdateOrderResult
        {
            Success = true,
            CurrentOrder = order
        };
    }

    public UpdateOrderResult UpdateOrderRequest(int orderId, string updatedNotes, List<OrderItem> updatedItems, int expectedVersion, int staffId)
    {
        if (!_permissionService.CanUpdateOrder(staffId, orderId))
        {
            return new UpdateOrderResult
            {
                Success = false,
                Message = "You do not have permission to update this order."
            };
        }

        var order = _orderService.FindOrder(orderId);
        if (order is null)
        {
            return new UpdateOrderResult
            {
                Success = false,
                Message = "Order not found."
            };
        }

        if (!CheckOrderStatus(order))
        {
            return new UpdateOrderResult
            {
                Success = false,
                CurrentOrder = order,
                Message = "The order cannot be updated."
            };
        }

        if (!ValidateUpdatedData(orderId, updatedItems, updatedNotes, out var validationError))
        {
            return new UpdateOrderResult
            {
                Success = false,
                CurrentOrder = order,
                Message = validationError
            };
        }

        if (!_orderService.CheckVersion(orderId, expectedVersion))
        {
            return new UpdateOrderResult
            {
                Success = false,
                IsConflict = true,
                Message = "Concurrent modification conflict.",
                CurrentOrder = _orderService.FindOrder(orderId)
            };
        }

        var updateResult = UpdateOrder(order, updatedNotes, updatedItems, expectedVersion);
        if (string.Equals(updateResult, "Conflict", StringComparison.Ordinal))
        {
            return new UpdateOrderResult
            {
                Success = false,
                IsConflict = true,
                Message = "Concurrent modification conflict.",
                CurrentOrder = _orderService.FindOrder(orderId)
            };
        }

        RecordUpdateLog(orderId, staffId, "Update Order");
        _notificationService.NotifyOrderUpdated(order.GetStudentId(), orderId);

        return new UpdateOrderResult
        {
            Success = true,
            CurrentOrder = order,
            Message = "Order updated successfully."
        };
    }

    public ViewOrderResult CancelUpdate()
    {
        return RetrieveOrderList();
    }

    private bool CheckOrderStatus(Order order)
    {
        return order.CheckUpdatableStatus();
    }

    private bool ValidateUpdatedData(int orderId, List<OrderItem> updatedItems, string updatedNotes, out string validationError)
    {
        if (!OrderItem.ValidateOrderItems(orderId, updatedItems))
        {
            validationError = "Invalid order item data.";
            return false;
        }

        return _validationService.Validate(updatedItems, updatedNotes, out validationError);
    }

    private string UpdateOrder(Order order, string updatedNotes, List<OrderItem> updatedItems, int expectedVersion)
    {
        order.UpdateOrder(updatedNotes);
        order.UpdateDetails(updatedItems, order.Notes);

        var saveSuccess = _orderService.Save(order, expectedVersion);
        return saveSuccess ? "Updated" : "Conflict";
    }

    private void RecordUpdateLog(int orderId, int staffId, string action)
    {
        _auditService.Log(action, orderId, staffId);
    }
}
