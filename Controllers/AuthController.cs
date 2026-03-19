using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpGet("")]
    [HttpGet("Login")]
    public IActionResult Login()
    {
        // If already logged in, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError(nameof(username), "Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(nameof(password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        // Get user by username
        var user = await _userService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        // Check if user account is active
        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Your account has been deactivated.");
            return View();
        }

        // Verify password
        if (!_userService.VerifyPassword(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        // Create claims from user record
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Role", user.Role),
            new Claim("UserId", user.UserId.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        // Redirect based on role
        return user.Role switch
        {
            "Student" => RedirectToAction("Index", "Order"),
            "CanteenStaff" => RedirectToAction("Index", "Order"),
            "Manager" => RedirectToAction("List", "Issue"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet("Register")]
    public IActionResult Register()
    {
        // If already logged in, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string phone, string role)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError(nameof(username), "Username is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(nameof(password), "Password is required.");
        }

        if (password != confirmPassword)
        {
            ModelState.AddModelError(nameof(confirmPassword), "Passwords do not match.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            ModelState.AddModelError(nameof(role), "Please select a role.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        // Register user
        var (success, message) = await _userService.RegisterUserAsync(username, email, password, phone ?? string.Empty, role);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("", message);
        return View();
    }
}
