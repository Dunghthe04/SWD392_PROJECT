namespace SWD392_PROJECT.Services;

public interface IStaffContext
{
    bool IsAuthenticated { get; }

    bool HasOrderManagementPermission { get; }

    int StaffId { get; }
}
