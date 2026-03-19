using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class ViewOrderCoordinator : IViewOrderCoordinator
{
    private readonly IOrderService _orderService;

    public ViewOrderCoordinator(IOrderService orderService)
    {
        _orderService = orderService;
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

        if (orderList.Count == 0)
        {
            return new ViewOrderResult
            {
                OrderList = new List<Models.Order>(),
                Message = "No orders are available."
            };
        }

        return new ViewOrderResult
        {
            OrderList = orderList
        };
    }
}
