using Ecommerce.DataAccess.Services.Cart;
using Ecommerce.Entities.DTO.Cart;
using Ecommerce.Entities.Shared.Bases;

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =("Buyer"))]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ResponseHandler _responseHandler;
        private readonly IValidator<AddToCartRequest> _addToCartValidator;
        private readonly IValidator<UpdateCartItemRequest> _updateCartItemValidator;

        public CartController(
            ICartService cartService,
            ResponseHandler responseHandler,
            IValidator<AddToCartRequest> addToCartValidator,
            IValidator<UpdateCartItemRequest> updateCartItemValidator)
        {
            _cartService = cartService;
            _responseHandler = responseHandler;
            _addToCartValidator = addToCartValidator;
            _updateCartItemValidator = updateCartItemValidator;
        }

        [HttpPost("add")]
        public async Task<ActionResult<Response<CartResponse>>> AddToCart([FromBody] AddToCartRequest request)
        {
            var validationResult = await _addToCartValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = await _cartService.AddToCartAsync(buyerId, request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        public async Task<ActionResult<Response<CartResponse>>> GetCart()
        {
            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = await _cartService.GetCartAsync(buyerId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update-quantity")]
        public async Task<ActionResult<Response<CartResponse>>> UpdateCartItemQuantity([FromBody] UpdateCartItemRequest request)
        {
            var validationResult = await _updateCartItemValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = await _cartService.UpdateCartItemQuantityAsync(buyerId, request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<ActionResult<Response<CartResponse>>> RemoveCartItem(Guid cartItemId)
        {
            var buyerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var response = await _cartService.RemoveCartItemAsync(buyerId, cartItemId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
