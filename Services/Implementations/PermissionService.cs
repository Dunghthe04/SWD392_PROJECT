using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class PermissionService : IPermissionService
{
    public bool CanUpdateOrder(int staffId, int orderId)
    {
        return staffId > 0;
    }
}
