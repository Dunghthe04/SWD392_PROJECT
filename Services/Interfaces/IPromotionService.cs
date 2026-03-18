using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Services.Interfaces
{
    /// <summary>
    /// IPromotionService - Business Logic Interface for Promotion Entity
    /// Defines contract for promotion business operations
    /// UC24 - View Promotions
    /// </summary>
    public interface IPromotionService
    {
        /// <summary>
        /// Get all promotions from the system
        /// </summary>
        /// <returns>List of all promotions</returns>
        Task<List<Promotion>> GetAllPromotionsAsync();

        /// <summary>
        /// Get all active (valid and non-expired) promotions
        /// </summary>
        /// <returns>List of active promotions</returns>
        Task<List<Promotion>> GetAllActivePromotionsAsync();

        /// <summary>
        /// Get promotion details by ID
        /// Checks if promotion exists and is not expired
        /// </summary>
        /// <param name="promotionId">The promotion ID</param>
        /// <returns>ServiceResult with promotion details or error</returns>
        Task<PromotionServiceResult> GetPromotionDetailAsync(int promotionId);

        /// <summary>
        /// Create a new promotion
        /// </summary>
        /// <param name="title">Promotion title</param>
        /// <param name="description">Promotion description</param>
        /// <param name="discountRate">Discount percentage</param>
        /// <param name="expiryDate">Promotion expiry date</param>
        /// <returns>ServiceResult with created promotion or error</returns>
        Task<PromotionServiceResult> CreatePromotionAsync(string title, string description, float discountRate, System.DateTime expiryDate);

        /// <summary>
        /// Update promotion details
        /// </summary>
        /// <param name="promotionId">Promotion ID to update</param>
        /// <param name="newDiscountRate">New discount rate</param>
        /// <param name="newExpiryDate">New expiry date</param>
        /// <returns>ServiceResult with updated promotion or error</returns>
        Task<PromotionServiceResult> UpdatePromotionAsync(int promotionId, float? newDiscountRate, System.DateTime? newExpiryDate);

        /// <summary>
        /// Delete a promotion
        /// </summary>
        /// <param name="promotionId">Promotion ID to delete</param>
        /// <returns>ServiceResult indicating success or error</returns>
        Task<PromotionServiceResult> DeletePromotionAsync(int promotionId);
    }

    /// <summary>
    /// Service result wrapper for promotion operations
    /// </summary>
    public class PromotionServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Promotion? Promotion { get; set; }
        public List<Promotion>? Promotions { get; set; }
    }
}
