using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using System.Security.Claims;

namespace SWD392_PROJECT.Controllers;

/// <summary>
/// ProfileController - Use-case coordinator for user profile management
/// Handles profile view and update operations
/// </summary>
[Authorize]
[Route("Profile")]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IUserService userService, ILogger<ProfileController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Display user profile information
    /// GET: /Profile/Index
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(user);
    }

    /// <summary>
    /// Display profile edit form
    /// GET: /Profile/Edit
    /// </summary>
    [HttpGet("Edit")]
    public async Task<IActionResult> Edit()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(user);
    }

    /// <summary>
    /// Process profile update request
    /// Implements submitUpdateRequest logic from design specification
    /// POST: /Profile/Update
    /// </summary>
    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromForm] string email, [FromForm] string phone)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Edit");
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Edit");
        }

        var (success, message) = await _userService.UpdateProfileAsync(userId, email, phone);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index");
        }
        else
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction("Edit");
        }
    }
}
