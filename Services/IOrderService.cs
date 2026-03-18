using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services;

public interface IOrderService
{
    List<Order>? ReadOrderList();

    Order? FindOrder(int orderId);

    bool CheckVersion(int orderId, int expectedVersion);

    bool Save(Order order, int expectedVersion);
}
