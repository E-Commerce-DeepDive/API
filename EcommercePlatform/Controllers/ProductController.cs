using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.DataAccess.Services.Admin;
using Ecommerce.Entities.DTO.Product;
using Ecommerce.Entities.Shared.Bases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProductsController : Controller
{
    
    private readonly IAdminService _adminService;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IAdminService adminService,
        ResponseHandler responseHandler,
        ILogger<ProductsController> logger)
    {
        _adminService = adminService;
        _responseHandler = responseHandler;
        _logger = logger;
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromForm] CreateProductRequest dto)
    {
        var response = await _adminService.AddProductAsync(dto);
        return StatusCode((int)response.StatusCode, response);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _adminService.GetProductsAsync(p => !p.IsDeleted && p.IsActive);
        return StatusCode((int)result.StatusCode, result);
    }

 
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _adminService.GetProductByIdAsync(id);
        return StatusCode((int)result.StatusCode, result);
    }
    
    [HttpPost("products/by-ids")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<List<GetProductResponse>>>> GetProductsByIds([FromBody] List<Guid> ids)
    {
        var result = await _adminService.GetProductsByIdsAsync(ids);
        return StatusCode((int)result.StatusCode, result);
    }

    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] UpdateProductRequest dto)
    {
        var result = await _adminService.UpdateProductAsync(id, dto);
        return StatusCode((int)result.StatusCode, result);
    }

  
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _adminService.DeleteProductAsync(id);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("category/{categoryName}")]
    public async Task<IActionResult> GetProductsByCategory(string categoryName)
    {
        var result = await _adminService.GetProductsByCategoryNameAsync(categoryName);
        return StatusCode((int)result.StatusCode, result);
    }

}