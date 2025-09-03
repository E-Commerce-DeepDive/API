using Ecommerce.DataAccess.Services.Payment;
using Ecommerce.Entities.DTO.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDto request)
        {
            var result = await _paymentService.CreatePaymentIntentAsync(request);
            return Ok(result);
        }

        [HttpPost("confirm/{paymentIntentId}")]
        public async Task<IActionResult> ConfirmPayment(string paymentIntentId)
        {
            var result = await _paymentService.ConfirmPaymentAsync(paymentIntentId);
            return Ok(result);
        }

        [HttpPost("refund/{paymentIntentId}")]
        public async Task<IActionResult> RefundPayment(string paymentIntentId, [FromQuery] decimal? amount)
        {
            var result = await _paymentService.RefundPaymentAsync(paymentIntentId, amount);
            return Ok(result);
        }
    }
}
