using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    List<Order>? ReadOrderList();

    Order? ReadOrder(int orderId);

    bool UpdateOrder(Order updatedOrder, int expectedVersion);

    void CreateOrder(Order order);

    void CreateOrderItems(List<OrderItem> orderItems);

    void UpdateOrderTotal(int orderId, decimal totalPrice, string status);

    bool DeleteOrder(int orderId);

    void AddAuditLog(AuditLog log);
}
