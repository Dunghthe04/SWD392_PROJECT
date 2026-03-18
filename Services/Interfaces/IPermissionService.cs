namespace SWD392_PROJECT.Services.Interfaces;

public interface IPermissionService
{
    bool CanUpdateOrder(int staffId, int orderId);
}
