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
public class AdminController : Controller
{
    
    private readonly IAdminService _adminService;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ResponseHandler responseHandler,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _responseHandler = responseHandler;
        _logger = logger;
    }
    
    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct([FromForm] CreateProductRequest dto)
    {
        var response = await _adminService.AddProductAsync(dto);
        return StatusCode((int)response.StatusCode, response);
    }
}