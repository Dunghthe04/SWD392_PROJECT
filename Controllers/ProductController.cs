using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Controllers;

// CLASS 1: ProductCoordinator («control»)
public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    // M1: Manager selects Create Product
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateProductViewModel
        {
            AvailableStalls = await _context.Stalls.ToListAsync()
        };
        return View(model); // M2: displays create product form
    }

    // M4: Manager submits the form -> M5: submitProductData(productData)
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductViewModel productData)
    {
        productData.AvailableStalls = await _context.Stalls.ToListAsync();

        // Chuyển tuyến gọi hàm UML chuẩn
        return await CreateProduct(productData);
    }

    // UML Method: +validateProduct(productData) : Boolean
    public bool ValidateProduct(CreateProductViewModel productData)
    {
        // M6: validates the input data (E1.1)
        if (string.IsNullOrWhiteSpace(productData.Name)) return false;
        if (productData.Price <= 0) return false;
        if (productData.Quantity < 0) return false;
        if (!ModelState.IsValid) return false;

        return true;
    }

    // UML Method: +createProduct(productData)
    public async Task<IActionResult> CreateProduct(CreateProductViewModel productData)
    {
        // 1. Xác thực dữ liệu
        bool isValid = ValidateProduct(productData);
        if (!isValid)
        {
            // Exception E1: Invalid Input Data
            // M6E.2: showInvalidInputMessage()
            productData.StatusMessage = "Invalid product information. Please re-enter the data.";
            productData.IsSuccess = false;
            return View("Create", productData); // M6E.3
        }

        try
        {
            // M7: checkProductNameExists(name)
            bool nameAlreadyExists = await _context.Products.AnyAsync(p => p.Name == productData.Name);
            
            // Alternative A1: Product Name Already Exists
            if (nameAlreadyExists) // M8a: returns nameAlreadyExists
            {
                // M9a: showDuplicateNameMessage()
                productData.StatusMessage = "Product name must be unique. This name already exists.";
                productData.IsSuccess = false;
                return View("Create", productData); // M10a: displays exist name
            }
            
            // M8: nameAvailable (Tiếp tục)

            // Bước tạo Object chuẩn theo Data Abstraction
            var product = new Product();
            product.SetProductInfo(productData); // M9: createProduct() ủy quyền cho Product Object
            product.CreateProduct();

            // M10: Lưu xuống DB -> returns productCreated(productId)
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // M11: createStock(productId, stallId, quantity) -> Entity StallProduct
            var stallProduct = new StallProduct();
            bool stockCreated = stallProduct.CreateStock(product.ProductId, productData.StallId, productData.Quantity); 
            // M12: returns stockCreated (boolean)

            _context.StallProducts.Add(stallProduct);
            await _context.SaveChangesAsync();

            // M13: showCreateSuccessMessage() 
            TempData["SuccessMessage"] = "Product created successfully!"; // M14
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            // Exception E2: System Failure
            // M9E.2: showSystemFailureMessage()
            productData.StatusMessage = "Product could not be created due to a system failure.";
            productData.IsSuccess = false;
            return View("Create", productData); // M9E.3
        }
    }
}
