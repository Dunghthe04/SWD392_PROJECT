using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly IOrderRepository _orderRepository;

    public AuditService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public void Log(string actionType, int orderId, int staffId)
    {
        _orderRepository.AddAuditLog(new AuditLog
        {
            ActionType = actionType,
            OrderId = orderId,
            StaffId = staffId,
            ActionTime = DateTime.UtcNow
        });
    }
}
