using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SWD392_PROJECT.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
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
    public async Task<IActionResult> Login(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            ModelState.AddModelError("", "Please select a role.");
            return View();
        }

        // Validate role
        var validRoles = new[] { "Student", "CanteenStaff", "Manager" };
        if (!validRoles.Contains(role))
        {
            ModelState.AddModelError("", "Invalid role selected.");
            return View();
        }

        // Create claims based on role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"User_{DateTime.Now.Ticks}"),
            new Claim(ClaimTypes.Role, role),
            new Claim("Role", role)
        };

        // For demo purposes, assign specific IDs based on role
        switch (role)
        {
            case "Student":
                claims.Add(new Claim("UserId", "2001"));
                claims.Add(new Claim("StaffId", "0")); // Not applicable for students
                break;
            case "CanteenStaff":
                claims.Add(new Claim("UserId", "0")); // Not applicable for staff
                claims.Add(new Claim("StaffId", "9001"));
                break;
            case "Manager":
                claims.Add(new Claim("UserId", "0")); // Not applicable for managers
                claims.Add(new Claim("StaffId", "9002"));
                break;
        }

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        // Redirect based on role
        return role switch
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
}
