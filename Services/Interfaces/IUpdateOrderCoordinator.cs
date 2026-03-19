using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services.Interfaces;

public interface IUpdateOrderCoordinator
{
    ViewOrderResult RetrieveOrderList();

    UpdateOrderResult RetrieveOrderInformation(int orderId);

    UpdateOrderResult UpdateOrderRequest(int orderId, string updatedNotes, List<OrderItem> updatedItems, int expectedVersion, int staffId);

    ViewOrderResult CancelUpdate();
}
