using Microsoft.Extensions.Logging;
using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Services.Implementations
{
    /// <summary>
    /// PromotionService - Business Logic Layer for Promotion Entity
    /// Handles all promotion-related business operations
    /// UC24 - View Promotions
    /// </summary>
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(IPromotionRepository promotionRepository, ILogger<PromotionService> logger)
        {
            _promotionRepository = promotionRepository ?? throw new ArgumentNullException(nameof(promotionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// UC24.1 - Get all promotions from the system
        /// Logic: Thực hiện findAll(Promotion) để lấy danh sách toàn bộ Promotion
        /// </summary>
        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all promotions from database");
                var promotions = await _promotionRepository.FindAll();
                _logger.LogInformation($"Successfully retrieved {promotions?.Count ?? 0} promotions");
                return promotions ?? new List<Promotion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all promotions");
                throw;
            }
        }

        /// <summary>
        /// UC24.1 - Get all active (valid and non-expired) promotions
        /// </summary>
        public async Task<List<Promotion>> GetAllActivePromotionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all active promotions");
                var activePromotions = await _promotionRepository.FindAllActive();
                _logger.LogInformation($"Successfully retrieved {activePromotions?.Count ?? 0} active promotions");
                return activePromotions ?? new List<Promotion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active promotions");
                throw;
            }
        }

        /// <summary>
        /// UC24.2 - Get promotion detail by ID
        /// Logic chuẩn theo pseudocode:
        /// 1. Truy vấn tìm Promotion bằng promotionId
        /// 2. Nếu promo != null:
        ///    - Kiểm tra xem khuyến mãi đã hết hạn chưa thông qua logic promo.isExpired()
        ///    - Nếu hết hạn -> Trả về thông báo lỗi "Offer Ended" (Exception 1)
        ///    - Nếu không hết hạn -> Trả về/hiển thị chi tiết promotion card
        /// 3. Nếu promo == null -> Trả về lỗi "Promotion Not Found"
        /// </summary>
        public async Task<PromotionServiceResult> GetPromotionDetailAsync(int promotionId)
        {
            try
            {
                _logger.LogInformation($"Fetching promotion details for ID: {promotionId}");

                // Step 1: Truy vấn tìm Promotion bằng promotionId
                var promotion = await _promotionRepository.FindByPromotionId(promotionId);

                // Step 2: Nếu promo == null -> Trả về lỗi "Promotion Not Found"
                if (promotion == null)
                {
                    _logger.LogWarning($"Promotion with ID {promotionId} not found");
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = $"Promotion with ID {promotionId} not found (Exception: E001)"
                    };
                }

                // Step 3: Kiểm tra xem khuyến mãi đã hết hạn chưa thông qua logic promo.isExpired()
                if (promotion.IsExpired())
                {
                    _logger.LogWarning($"Promotion with ID {promotionId} has expired");
                    // Exception 1: Offer Ended
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Offer Ended - This promotion has expired and is no longer available (Exception: E002)"
                    };
                }

                // Step 4: Nếu không hết hạn -> Trả về chi tiết promotion card
                _logger.LogInformation($"Successfully retrieved promotion details for ID: {promotionId}");
                return new PromotionServiceResult
                {
                    Success = true,
                    Message = "Promotion details retrieved successfully",
                    Promotion = promotion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching promotion detail for ID {promotionId}");
                return new PromotionServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while retrieving promotion details: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create a new promotion
        /// Validation: Title and discountRate are required, ExpiryDate must be in the future
        /// </summary>
        public async Task<PromotionServiceResult> CreatePromotionAsync(string title, string description, float discountRate, DateTime expiryDate)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(title))
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Promotion title is required and cannot be empty"
                    };
                }

                if (discountRate <= 0 || discountRate > 100)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Discount rate must be between 0.01 and 100 percent"
                    };
                }

                if (expiryDate <= DateTime.UtcNow)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Expiry date must be in the future"
                    };
                }

                // Create promotion using factory method
                var promotion = Promotion.CreatePromotion(title, description, discountRate, expiryDate);

                // Save to database
                var savedPromotion = await _promotionRepository.Create(promotion);

                _logger.LogInformation($"Successfully created promotion with ID: {savedPromotion.PromotionId}");

                return new PromotionServiceResult
                {
                    Success = true,
                    Message = "Promotion created successfully",
                    Promotion = savedPromotion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating promotion");
                return new PromotionServiceResult
                {
                    Success = false,
                    Message = $"Error creating promotion: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update promotion discount rate and/or expiry date
        /// </summary>
        public async Task<PromotionServiceResult> UpdatePromotionAsync(int promotionId, float? newDiscountRate, DateTime? newExpiryDate)
        {
            try
            {
                // Validate ID
                if (promotionId <= 0)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Invalid promotion ID"
                    };
                }

                // Retrieve existing promotion
                var promotion = await _promotionRepository.FindByPromotionId(promotionId);

                if (promotion == null)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = $"Promotion with ID {promotionId} not found"
                    };
                }

                // Update using business logic method
                promotion.UpdatePromotion(newDiscountRate, newExpiryDate);

                // Save to database
                var updatedPromotion = await _promotionRepository.Update(promotion);

                _logger.LogInformation($"Successfully updated promotion with ID: {promotionId}");

                return new PromotionServiceResult
                {
                    Success = true,
                    Message = "Promotion updated successfully",
                    Promotion = updatedPromotion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating promotion with ID {promotionId}");
                return new PromotionServiceResult
                {
                    Success = false,
                    Message = $"Error updating promotion: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete a promotion (soft delete)
        /// </summary>
        public async Task<PromotionServiceResult> DeletePromotionAsync(int promotionId)
        {
            try
            {
                // Validate ID
                if (promotionId <= 0)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = "Invalid promotion ID"
                    };
                }

                // Delete from repository
                var deleted = await _promotionRepository.Delete(promotionId);

                if (!deleted)
                {
                    return new PromotionServiceResult
                    {
                        Success = false,
                        Message = $"Promotion with ID {promotionId} not found"
                    };
                }

                _logger.LogInformation($"Successfully deleted promotion with ID: {promotionId}");

                return new PromotionServiceResult
                {
                    Success = true,
                    Message = "Promotion deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting promotion with ID {promotionId}");
                return new PromotionServiceResult
                {
                    Success = false,
                    Message = $"Error deleting promotion: {ex.Message}"
                };
            }
        }
    }
}
