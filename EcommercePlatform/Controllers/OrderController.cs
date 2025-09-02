using Ecommerce.API.Extensions;
using Ecommerce.DataAccess.Services.Order;
using Ecommerce.Entities.DTO.Order;
using Ecommerce.Entities.DTO.Shared;
using Ecommerce.Entities.Shared;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Utilities.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<UpdateOrderRequest> _updateRequestValidator;
    private readonly ResponseHandler _responseHandler;

    public OrderController(
        IOrderService orderService,
        IValidator<UpdateOrderRequest> updateRequestValidator,
        ResponseHandler responseHandler) 
    {
        _orderService = orderService;
        _updateRequestValidator = updateRequestValidator;
        _responseHandler = responseHandler;
    }

    [HttpPost("buyer/create")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Response<OrderResponse>>> AddOrder([FromBody] OrderRequest request,
        CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

        var userId = User.GetUserId();

        var response = await _orderService.AddOrderAsync(request, userId!, cancellationToken);

        return StatusCode((int)response.StatusCode, response);
    }
    
    [HttpGet("buyer/list")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Response<PaginatedList<OrderResponse>>>> GetAll([FromQuery] OrderFilters<OrderSorting> filters, CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

        var userId = User.GetUserId();
        var response = await _orderService.GetAllAsync(userId!, filters, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }
    
    [HttpPut("admin/{orderId}/update")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response<bool>>> UpdateOrder([FromRoute] string orderId, [FromBody] UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

        var validation = await _updateRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            // map FluentValidation failures to ModelState
            foreach (var failure in validation.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }

            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));
        }

        var response = await _orderService.UpdateOrderAsync(orderId, request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }


    [HttpPut("admin/{id}/confirm")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response<bool>>> ConfirmOrder([FromRoute] string id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

        var response = await _orderService.ConfirmOrderAsync(id, cancellationToken);

        return StatusCode((int)response.StatusCode, response);
    }
    
    // Buyer delete (only cancelled)
    [HttpDelete("buyer/{id}/delete")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<Response<bool>>> DeleteOrderAsBuyer([FromRoute] string id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _orderService.DeleteOrderAsync(id, userId!, false, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }


    [HttpPut("buyer/{id}/cancel")]
    [HttpPut("admin/{id}/cancel")]
    [Authorize(Roles = "Buyer,Admin")]
    public async Task<ActionResult<Response<bool>>> CancelOrder([FromRoute] string id, [FromBody] AdminCancelOrderRequest request, CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var response = await _orderService.CancelOrderAsync(id, userId!, isAdmin, request, cancellationToken);

        return StatusCode((int)response.StatusCode, response);
    }
    
    /// Admin Endpoints
    public record AdminCreateOrderRequest(string BuyerId, OrderRequest OrderRequest);

    [HttpPost("admin/create-for-buyer")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response<OrderResponse>>> CreateOrderForBuyer([FromBody] AdminCreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

       
        if (!Guid.TryParse(request.BuyerId, out _))
            return BadRequest(_responseHandler.BadRequest<OrderResponse>("Invalid buyer id format"));

        var response = await _orderService.AddOrderAsync(request.OrderRequest, request.BuyerId, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }
    
    [HttpGet("admin/list")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response<PaginatedList<OrderResponse>>>> GetAllForAdmin([FromQuery] OrderFilters<OrderSorting> filters, CancellationToken cancellationToken)
    {
        var response = await _orderService.GetAllAsync(null, filters, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }
    
    
    // Admin delete
    [HttpDelete("admin/{id}/delete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response<bool>>> DeleteOrderAsAdmin([FromRoute] string id, CancellationToken cancellationToken)
    {
        var response = await _orderService.DeleteOrderAsync(id, User.GetUserId()!, true, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }



}