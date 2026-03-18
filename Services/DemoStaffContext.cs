namespace SWD392_PROJECT.Services;

public class DemoStaffContext : IStaffContext
{
    public bool IsAuthenticated => true;

    public bool HasOrderManagementPermission => true;

    public int StaffId => 9001;
}
