using System.Security.Claims;
using Ecommerce.DataAccess.Services.Review;
using Ecommerce.Entities.DTO.Review.Ecommerce.Entities.DTO.Reviews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequestDto dto)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _reviewService.AddReviewAsync( dto);
            return Ok(result);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProduct(Guid productId)
        {
            var result = await _reviewService.GetReviewsByProductAsync(productId);
            return Ok(result);
        }

        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedProducts([FromQuery] int count = 5)
        {
            var result = await _reviewService.GetTopRatedProductsAsync(count);
            return Ok(result);
        }
    }
}
