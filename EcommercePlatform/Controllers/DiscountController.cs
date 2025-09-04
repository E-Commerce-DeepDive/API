using Ecommerce.DataAccess.Services.Discount;
using Ecommerce.Entities.DTO.Discount;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateDiscountDto dto)
        {
            var response = await _discountService.CreateDiscountAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var response = await _discountService.GetAllDiscountsAsync(status);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _discountService.GetDiscountByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateDiscountDto dto)
        {
            var response = await _discountService.UpdateDiscountAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _discountService.DeleteDiscountAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyDiscountDto dto)
        {
            var response = await _discountService.ApplyDiscountAsync(dto.Code, dto.CartTotal, dto.ProductIds, dto.CategoryIds);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
