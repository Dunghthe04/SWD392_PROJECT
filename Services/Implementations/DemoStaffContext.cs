using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class DemoStaffContext : IStaffContext
{
    public bool IsAuthenticated => true;

    public bool HasOrderManagementPermission => true;

    public int StaffId => 9001;
}
