using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Services.Implementations;

public class ValidationService : IValidationService
{
    public bool Validate(List<OrderItem> updatedItems, string notes, out string errorMessage)
    {
        if (updatedItems.Count == 0)
        {
            errorMessage = "Order must contain at least one item.";
            return false;
        }

        if (updatedItems.Any(item => item.Quantity <= 0))
        {
            errorMessage = "Each item quantity must be greater than 0.";
            return false;
        }

        if (notes.Length > 500)
        {
            errorMessage = "Notes cannot exceed 500 characters.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
