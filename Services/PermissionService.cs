namespace SWD392_PROJECT.Services;

public class PermissionService : IPermissionService
{
    public bool CanUpdateOrder(int staffId, int orderId)
    {
        return staffId > 0;
    }
}
