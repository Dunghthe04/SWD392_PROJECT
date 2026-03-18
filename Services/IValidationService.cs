using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services;

public interface IValidationService
{
    bool Validate(List<OrderItem> updatedItems, string notes, out string errorMessage);
}
