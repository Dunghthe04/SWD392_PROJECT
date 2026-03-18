using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Coordinators;

public class UpdateOrderCoordinator
{
    private readonly IPermissionService _permissionService;
    private readonly IValidationService _validationService;
    private readonly IOrderService _orderService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public UpdateOrderCoordinator(
        IPermissionService permissionService,
        IValidationService validationService,
        IOrderService orderService,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _permissionService = permissionService;
        _validationService = validationService;
        _orderService = orderService;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public UpdateOrderResult ProcessUpdateOrder(
        int orderId,
        int staffId,
        List<OrderItem> updatedItems,
        string notes,
        int expectedVersion)
    {
        var accessGranted = _permissionService.CanUpdateOrder(staffId, orderId);
        if (!accessGranted)
        {
            return new UpdateOrderResult { Message = "You do not have permission to update this order." };
        }

        var order = _orderService.FindOrder(orderId);
        if (order is null)
        {
            return new UpdateOrderResult { Message = "Order not found." };
        }

        if (!order.IsUpdatable())
        {
            return new UpdateOrderResult
            {
                Message = "This order cannot be updated because it is already processed, cancelled, or locked.",
                CurrentOrder = order
            };
        }

        var validationPassed = _validationService.Validate(updatedItems, notes, out var validationError);
        if (!validationPassed)
        {
            return new UpdateOrderResult
            {
                Message = validationError,
                CurrentOrder = order
            };
        }

        var saveAllowed = _orderService.CheckVersion(orderId, expectedVersion);
        if (!saveAllowed)
        {
            var latestOrder = _orderService.FindOrder(orderId);
            return new UpdateOrderResult
            {
                IsConflict = true,
                Message = "Update conflict detected. The order was changed by another process. Please review the latest data and try again.",
                CurrentOrder = latestOrder
            };
        }

        order.UpdateDetails(updatedItems, notes);

        var saved = _orderService.Save(order, expectedVersion);
        if (!saved)
        {
            var latestOrder = _orderService.FindOrder(orderId);
            return new UpdateOrderResult
            {
                IsConflict = true,
                Message = "Update conflict detected. Please reload the order and try again.",
                CurrentOrder = latestOrder
            };
        }

        _auditService.Log("Update Order", orderId, staffId);
        var studentId = order.GetStudentId();
        _notificationService.NotifyOrderUpdated(studentId, orderId);

        return new UpdateOrderResult
        {
            Success = true,
            Message = "Order updated successfully."
        };
    }
}
