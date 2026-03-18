namespace SWD392_PROJECT.Services;

public interface IPermissionService
{
    bool CanUpdateOrder(int staffId, int orderId);
}
