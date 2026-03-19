using Microsoft.AspNetCore.Mvc;
using SWD392_PROJECT.Services.Interfaces;

namespace SWD392_PROJECT.Controllers;

/// <summary>
/// Controller for browsing and viewing menu items
/// </summary>
public class MenuController : Controller
{
    private readonly IProductService _productService;

    public MenuController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Display all products/menu items
    /// </summary>
    public async Task<IActionResult> Browse(string? category, string? search)
    {
        try
        {
            IEnumerable<Models.Product> products;

            if (!string.IsNullOrEmpty(search))
            {
                System.Diagnostics.Debug.WriteLine($"[Browse] Searching for: {search}");
                products = await _productService.SearchProductsAsync(search);
            }
            else if (!string.IsNullOrEmpty(category))
            {
                System.Diagnostics.Debug.WriteLine($"[Browse] Getting products by category: {category}");
                products = await _productService.GetProductsByCategoryAsync(category);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Browse] Getting all products");
                products = await _productService.GetAllProductsAsync();
            }

            System.Diagnostics.Debug.WriteLine($"[Browse] Products count: {products.Count()}");

            var categories = await _productService.GetCategoriesAsync();
            System.Diagnostics.Debug.WriteLine($"[Browse] Categories count: {categories.Count()}");

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.ProductCount = products.Count(); // Debug info

            return View(products.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Browse] Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[Browse] Inner exception: {ex.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($"[Browse] Stack trace: {ex.StackTrace}");

            // Log exception here
            TempData["ErrorMessage"] = $"Error loading menu items: {ex.InnerException?.Message ?? ex.Message}";
            return View(new List<Models.Product>());
        }
    }

    /// <summary>
    /// Display product details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        catch (Exception ex)
        {
            // Log exception here
            TempData["ErrorMessage"] = "Error loading product details";
            return RedirectToAction("Browse");
        }
    }

    /// <summary>
    /// Debug action to check database
    /// </summary>
    public async Task<IActionResult> Debug()
    {
        var debugInfo = new
        {
            TotalProducts = await _productService.GetAllProductsAsync(),
            Categories = await _productService.GetCategoriesAsync()
        };
        return Json(debugInfo);
    }
}
