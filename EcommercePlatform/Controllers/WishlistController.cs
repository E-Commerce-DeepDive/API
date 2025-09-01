using System.Security.Claims;
using Ecommerce.DataAccess.Services.Wishlist;

using Ecommerce.Entities.DTO.Wishlist;
using Ecommerce.Entities.Shared.Bases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/buyer/wishlist")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly IValidator<WishlistAddItemRequest> _addValidator;
        private readonly IValidator<WishlistRemoveItemRequest> _removeValidator;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(
            IWishlistService wishlistService,
            IValidator<WishlistAddItemRequest> addValidator,
            IValidator<WishlistRemoveItemRequest> removeValidator,
            ResponseHandler responseHandler,
            ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _addValidator = addValidator;
            _removeValidator = removeValidator;
            _responseHandler = responseHandler;
            _logger = logger;
        }


        [HttpGet("view")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<Response<WishlistViewResponse>>> ViewWishlist()
        {

            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Starting ViewWishlist endpoint for BuyerId: {BuyerId}", buyerId);

            var response = await _wishlistService.ViewWishlistAsync(buyerId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("add-wishlist-item")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<Response<string>>> AddToWishlist([FromBody] WishlistAddItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid or null request received in AddToWishlistAsync.");
                return _responseHandler.BadRequest<string>("Invalid request: ProductId is required.");
            }
            var validation = await _addValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                string errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<string>(errors).StatusCode,
                                  _responseHandler.BadRequest<string>(errors));
            }

            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(buyerId))
            {
                _logger.LogWarning("Unauthorized access attempt: BuyerId not found in claims.");
                return StatusCode((int)_responseHandler.Unauthorized<string>("Unauthorized access.").StatusCode,
                                  _responseHandler.Unauthorized<string>("Unauthorized access."));
            }
            var response = await _wishlistService.AddToWishlistAsync(buyerId, request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("remove-wishlist-item")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<Response<string>>> RemoveFromWishlist([FromBody] WishlistRemoveItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid or null request received in AddToWishlistAsync.");
                return _responseHandler.BadRequest<string>("Invalid request: ProductId is required.");
            }
            var validation = await _removeValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                string errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<string>(errors).StatusCode,
                                  _responseHandler.BadRequest<string>(errors));
            }

            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(buyerId))
            {
                _logger.LogWarning("Unauthorized access attempt: BuyerId not found in claims.");
                return StatusCode((int)_responseHandler.Unauthorized<string>("Unauthorized access.").StatusCode,
                                  _responseHandler.Unauthorized<string>("Unauthorized access."));
            }
            var response = await _wishlistService.RemoveFromWishlistAsync(buyerId, request);
            return StatusCode((int)response.StatusCode, response);
        }

    }
}