using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Coordinators;

public class ViewOrderCoordinator
{
    private readonly IOrderService _orderService;

    public ViewOrderCoordinator(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public ViewOrderResult RetrieveOrderList()
    {
        try
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
                    Message = "No orders are available."
                };
            }

            return new ViewOrderResult
            {
                OrderList = orderList
            };
        }
        catch
        {
            return new ViewOrderResult
            {
                IsError = true,
                Message = "Unable to retrieve the order list."
            };
        }
    }
}
