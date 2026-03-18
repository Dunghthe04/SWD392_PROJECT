namespace SWD392_PROJECT.Services.Interfaces;

public interface IStaffContext
{
    bool IsAuthenticated { get; }

    bool HasOrderManagementPermission { get; }

    int StaffId { get; }
}
