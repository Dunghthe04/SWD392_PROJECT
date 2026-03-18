using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Data;

public class InMemoryOrderRepository : IOrderRepository
{
    private static readonly object SyncLock = new();
    private static readonly List<Order> Orders = BuildSeedOrders();
    private static readonly List<AuditLog> AuditLogs = new();
    private static int _nextAuditLogId = 1;

    public List<Order>? ReadOrderList()
    {
        lock (SyncLock)
        {
            return Orders.Select(order => order.Clone()).ToList();
        }
    }

    public Order? ReadOrder(int orderId)
    {
        lock (SyncLock)
        {
            var order = Orders.FirstOrDefault(item => item.OrderId == orderId);
            return order?.Clone();
        }
    }

    public bool UpdateOrder(Order updatedOrder, int expectedVersion)
    {
        lock (SyncLock)
        {
            var existingOrder = Orders.FirstOrDefault(item => item.OrderId == updatedOrder.OrderId);
            if (existingOrder is null)
            {
                return false;
            }

            if (existingOrder.Version != expectedVersion)
            {
                return false;
            }

            existingOrder.UpdateDetails(updatedOrder.Items, updatedOrder.Notes);
            existingOrder.Status = updatedOrder.Status;
            existingOrder.IsLocked = updatedOrder.IsLocked;
            existingOrder.Version += 1;
            existingOrder.LastUpdatedAt = DateTime.UtcNow;
            return true;
        }
    }

    public void CreateOrder(Order order)
    {
        lock (SyncLock)
        {
            if (Orders.Any(item => item.OrderId == order.OrderId))
            {
                throw new InvalidOperationException($"Order ID {order.OrderId} already exists.");
            }

            Orders.Add(order.Clone());
        }
    }

    public bool DeleteOrder(int orderId)
    {
        lock (SyncLock)
        {
            var order = Orders.FirstOrDefault(item => item.OrderId == orderId);
            if (order is null)
            {
                return false;
            }

            return Orders.Remove(order);
        }
    }

    public void AddAuditLog(AuditLog log)
    {
        lock (SyncLock)
        {
            log.AuditLogId = _nextAuditLogId;
            _nextAuditLogId += 1;
            AuditLogs.Add(log);
        }
    }

    private static List<Order> BuildSeedOrders()
    {
        return
        [
            Order.CreateOrder(
                orderId: 1001,
                studentId: 2001,
                studentName: "Nguyen Van A",
                targetItems:
                [
                    new OrderItem { MenuItemId = 1, ItemName = "Com ga", Quantity = 1, UnitPrice = 35000m },
                    new OrderItem { MenuItemId = 2, ItemName = "Tra dao", Quantity = 1, UnitPrice = 15000m }
                ],
                notes: "Khong hanh"
            ),
            Order.CreateOrder(
                orderId: 1002,
                studentId: 2002,
                studentName: "Tran Thi B",
                targetItems:
                [
                    new OrderItem { MenuItemId = 3, ItemName = "Bun bo", Quantity = 2, UnitPrice = 40000m }
                ],
                notes: "Them ot"
            )
        ];
    }
}
