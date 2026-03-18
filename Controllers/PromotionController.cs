using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Controllers
{
    /// <summary>
    /// PromotionController - StudentInteraction Role
    /// Handles all user interactions related to promotions
    /// UC24 - View Promotions
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(IPromotionService promotionService, ILogger<PromotionController> logger)
        {
            _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// UC24.1 - Display list of all promotions
        /// HTTP GET /Promotion/List
        /// Description: Thực hiện findAll(Promotion) để lấy danh sách toàn bộ Promotion
        ///             và hiển thị dưới dạng danh sách (List) trên giao diện của người dùng
        /// </summary>
        [HttpGet("List")]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                _logger.LogInformation("User requesting all promotions list");
                var promotions = await _promotionService.GetAllPromotionsAsync();
                
                return View("List", promotions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving promotions list");
                TempData["ErrorMessage"] = "An error occurred while retrieving promotions. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// UC24.1 - Display list of active promotions
        /// HTTP GET /Promotion/Active
        /// Only shows valid, non-expired promotions
        /// </summary>
        [HttpGet("Active")]
        public async Task<IActionResult> GetActivePromotions()
        {
            try
            {
                _logger.LogInformation("User requesting active promotions list");
                var activePromotions = await _promotionService.GetAllActivePromotionsAsync();
                
                return View("List", activePromotions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active promotions list");
                TempData["ErrorMessage"] = "An error occurred while retrieving active promotions. Please try again.";
                return RedirectToAction("GetAllPromotions");
            }
        }

        /// <summary>
        /// UC24.2 - Display promotion detail
        /// HTTP GET /Promotion/Details/{promotionId}
        /// Description: Thực hiện findById(Promotion, promotionId) để tìm Promotion theo ID
        ///             Kiểm tra xem khuyến mãi đã hết hạn chưa thông qua logic promo.isExpired()
        ///             Nếu hết hạn -> Trả về thông báo lỗi "Offer Ended" (Exception 1)
        ///             Nếu không hết hạn -> Trả về/hiển thị chi tiết promotion card
        /// </summary>
        [HttpGet("Details/{promotionId}")]
        public async Task<IActionResult> GetPromotionDetail(int promotionId)
        {
            try
            {
                _logger.LogInformation($"User requesting promotion detail for ID: {promotionId}");

                // Call service method that implements UC24.2 logic
                var result = await _promotionService.GetPromotionDetailAsync(promotionId);

                if (!result.Success)
                {
                    _logger.LogWarning($"Failed to retrieve promotion detail. Message: {result.Message}");
                    
                    // If offer ended, show specific error message
                    if (!string.IsNullOrEmpty(result.Message) && result.Message.Contains("Offer Ended"))
                    {
                        TempData["ErrorMessage"] = "This promotion has ended and is no longer available.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = result.Message ?? "Unable to retrieve promotion details.";
                    }
                    
                    return RedirectToAction("GetAllPromotions");
                }

                // Success - return promotion detail view
                _logger.LogInformation($"Successfully retrieved promotion detail for ID: {promotionId}");
                return View("Details", result.Promotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving promotion detail for ID: {promotionId}");
                TempData["ErrorMessage"] = "An error occurred while retrieving promotion details. Please try again.";
                return RedirectToAction("GetAllPromotions");
            }
        }
    }
}
