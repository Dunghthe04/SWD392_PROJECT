using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Controllers;

/// <summary>
/// Controller for UC11 - Report Issue
/// Handles issue reporting, evidence upload, and issue management
/// </summary>
[Authorize]
[Route("[controller]")]
public class IssueController : Controller
{
    private readonly IReportIssueService _reportIssueService;
    private readonly IOrderRepository _orderRepository;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ILogger<IssueController> _logger;

    public IssueController(
        IReportIssueService reportIssueService,
        IOrderRepository orderRepository,
        IWebHostEnvironment hostEnvironment,
        ILogger<IssueController> logger)
    {
        _reportIssueService = reportIssueService;
        _orderRepository = orderRepository;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// GET: Issue/List - Display list of issues
    /// </summary>
    [Authorize(Roles = "Manager")]
    [HttpGet("List")]
    public async Task<IActionResult> List(string? status = null)
    {
        try
        {
            List<Issue> issues;

            if (!string.IsNullOrEmpty(status))
            {
                issues = await _reportIssueService.GetIssuesByStatusAsync(status);
            }
            else
            {
                issues = await _reportIssueService.GetAllIssuesAsync();
            }

            var viewModel = new IssueListViewModel
            {
                Issues = issues.Select(i => new IssueListItemViewModel
                {
                    IssueId = i.IssueId,
                    OrderId = i.OrderId,
                    StudentName = i.Order?.StudentName ?? "Unknown",
                    Details = i.Details,
                    Status = i.Status,
                    CreatedDate = i.CreatedDate,
                    NotificationCount = i.Notifications?.Count ?? 0
                }).ToList(),
                TotalCount = issues.Count,
                FilterStatus = status
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading issues");
            TempData["Error"] = "Error loading issues";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// GET: Issue/Report/{orderId} - Display issue report form
    /// </summary>
    [HttpGet("Report/{orderId}")]
    public IActionResult Report(int orderId)
    {
        try
        {
            var order = _orderRepository.ReadOrder(orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction("Index", "Order");
            }

            var viewModel = new ReportIssueViewModel
            {
                OrderId = orderId,
                StudentName = order.StudentName,
                OrderDescription = $"Order #{order.OrderId} - {order.Status}"
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report form");
            TempData["Error"] = "Error loading report form";
            return RedirectToAction("Index", "Order");
        }
    }

    /// <summary>
    /// POST: Issue/Report/{orderId} - Submit issue report
    /// </summary>
    [HttpPost("Report/{orderId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportSubmit(int orderId, ReportIssueViewModel model)
    {
        try
        {
            // Ensure orderId in model matches URL
            model.OrderId = orderId;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                return View("Report", model);
            }

            // Submit issue (StudentId will be extracted from Order in service)
            var result = await _reportIssueService.SubmitIssueAsync(
                model.OrderId,
                model.IssueDetails
            );

            if (!result.Success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = result.Message });
                }
                ModelState.AddModelError("", result.Message);
                return View("Report", model);
            }

            // Handle image upload if provided
            if (model.EvidenceImage != null)
            {
                var uploadResult = await UploadEvidenceImageAsync(result.IssueId!.Value, model.EvidenceImage);
                if (!uploadResult.Success)
                {
                    _logger.LogWarning($"Image upload failed for issue {result.IssueId}: {uploadResult.Message}");
                }
            }

            // Return JSON response for AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    message = "Issue reported successfully. Managers will review it shortly.",
                    issueId = result.IssueId
                });
            }

            // For non-AJAX requests, use redirect
            TempData["Success"] = "Issue reported successfully. Managers will review it shortly.";
            return RedirectToAction("Details", new { id = result.IssueId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting issue report");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "An error occurred while submitting the report. Please try again." });
            }
            ModelState.AddModelError("", "An error occurred while submitting the report");
            return View("Report", model);
        }
    }

    /// <summary>
    /// GET: Issue/Details/{id} - Display issue details
    /// </summary>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var issue = await _reportIssueService.GetIssueByIdAsync(id);
            if (issue == null)
            {
                TempData["Error"] = "Issue not found";
                return RedirectToAction("List");
            }

            var viewModel = new IssueDetailViewModel
            {
                IssueId = issue.IssueId,
                OrderId = issue.OrderId,
                Details = issue.Details,
                Status = issue.Status,
                ImagePath = issue.ImagePath,
                CreatedDate = issue.CreatedDate,
                LastUpdatedDate = issue.LastUpdatedDate,
                StudentName = issue.Order?.StudentName ?? "Unknown",
                Notifications = issue.Notifications?.Select(n => new NotificationViewModel
                {
                    NotificationId = n.NotificationId,
                    IssueId = n.IssueId,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate
                }).ToList() ?? new()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading issue details");
            TempData["Error"] = "Error loading issue details";
            return RedirectToAction("List");
        }
    }

    /// <summary>
    /// POST: Issue/UpdateStatus - Update issue status
    /// </summary>
    [HttpPost("UpdateStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateIssueStatusViewModel model)
    {
        try
        {
            var result = await _reportIssueService.UpdateIssueStatusAsync(model.IssueId, model.NewStatus);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Details", new { id = model.IssueId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating issue status");
            TempData["Error"] = "An error occurred while updating the issue";
            return RedirectToAction("Details", new { id = model.IssueId });
        }
    }

    /// <summary>
    /// GET: Issue/Notifications - Display manager notifications
    /// </summary>
    [Authorize(Roles = "Manager")]
    [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications()
    {
        try
        {
            var notifications = await _reportIssueService.GetUnreadNotificationsAsync();

            var viewModel = notifications.Select(n => new NotificationViewModel
            {
                NotificationId = n.NotificationId,
                IssueId = n.IssueId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notifications");
            TempData["Error"] = "Error loading notifications";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// GET: Issue/GetUnreadCount - Get unread notification count (API)
    /// </summary>
    [Authorize(Roles = "Manager")]
    [HttpGet("GetUnreadCount")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var notifications = await _reportIssueService.GetUnreadNotificationsAsync();
            var count = notifications.Count;
            return Json(new { success = true, count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return Json(new { success = false, count = 0 });
        }
    }

    /// <summary>
    /// GET: Issue/GetUnreadNotifications - Get unread notifications list (API)
    /// </summary>
    [Authorize(Roles = "Manager")]
    [HttpGet("GetUnreadNotifications")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var notifications = await _reportIssueService.GetUnreadNotificationsAsync();
            var viewModel = notifications.Select(n => new
            {
                notificationId = n.NotificationId,
                issueId = n.IssueId,
                message = n.Message,
                isRead = n.IsRead,
                createdDate = n.CreatedDate.ToString("MMM dd, yyyy HH:mm"),
                createdDateRaw = n.CreatedDate
            }).OrderByDescending(n => n.createdDateRaw).ToList();

            return Json(new { success = true, notifications = viewModel });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading unread notifications");
            return Json(new { success = false, notifications = new List<object>() });
        }
    }

    /// <summary>
    /// POST: Issue/MarkNotificationRead - Mark notification as read
    /// </summary>
    [HttpPost("MarkNotificationRead/{notificationId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(int notificationId)
    {
        try
        {
            await _reportIssueService.MarkNotificationAsReadAsync(notificationId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return BadRequest("Error marking notification as read");
        }
    }

    /// <summary>
    /// Helper method to upload evidence image
    /// </summary>
    private async Task<ImageUploadResult> UploadEvidenceImageAsync(int issueId, IFormFile imageFile)
    {
        try
        {
            if (imageFile.Length == 0)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    Message = "File is empty"
                };
            }

            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "issues");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{issueId}_{DateTime.Now.Ticks}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            var relativePath = $"/uploads/issues/{uniqueFileName}";
            var result = await _reportIssueService.UploadEvidenceAsync(issueId, relativePath);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return new ImageUploadResult
            {
                Success = false,
                Message = $"Error uploading image: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// GET: Issue/GetNotificationCount - Get unread notification count (API)
    /// </summary>
    [Authorize(Roles = "Manager")]
    [HttpGet("GetNotificationCount")]
    public async Task<IActionResult> GetNotificationCount()
    {
        try
        {
            var notifications = await _reportIssueService.GetUnreadNotificationsAsync();
            return Json(new { count = notifications.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification count");
            return Json(new { count = 0 });
        }
    }
}
