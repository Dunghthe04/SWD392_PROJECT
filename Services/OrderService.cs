using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public List<Order>? ReadOrderList()
    {
        return _orderRepository.ReadOrderList();
    }

    public Order? FindOrder(int orderId)
    {
        return _orderRepository.ReadOrder(orderId);
    }

    public bool CheckVersion(int orderId, int expectedVersion)
    {
        var order = _orderRepository.ReadOrder(orderId);
        return order is not null && order.Version == expectedVersion;
    }

    public bool Save(Order order, int expectedVersion)
    {
        return _orderRepository.UpdateOrder(order, expectedVersion);
    }
}
