using System.Security.Claims;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class DemoStaffContext : IStaffContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DemoStaffContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? HttpContext => _httpContextAccessor?.HttpContext;

    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasOrderManagementPermission
    {
        get
        {
            if (!IsAuthenticated)
                return false;

            var role = HttpContext?.User?.FindFirst("Role")?.Value;
            // Both CanteenStaff and Manager can manage orders
            return role == "CanteenStaff" || role == "Manager";
        }
    }

    public int StaffId
    {
        get
        {
            if (!IsAuthenticated)
                return 0;

            var staffIdClaim = HttpContext?.User?.FindFirst("StaffId")?.Value;
            if (int.TryParse(staffIdClaim, out var staffId))
            {
                return staffId;
            }
            return 0;
        }
    }
}
