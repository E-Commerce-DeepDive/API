using Ecommerce.DataAccess.Services.Payment;
using Ecommerce.Entities.DTO.Payment;
using Microsoft.Extensions.Configuration;
using Stripe;

public class StripePaymentService : IPaymentService
{
    public StripePaymentService(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100),
            Currency = request.Currency,
            Description = request.Description,
            ReceiptEmail = request.CustomerEmail,
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return new PaymentResponseDto
        {
            Success = true,
            Message = "PaymentIntent created successfully",
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret
        };
    }

    public async Task<PaymentResponseDto> ConfirmPaymentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.ConfirmAsync(paymentIntentId);

        return new PaymentResponseDto
        {
            Success = paymentIntent.Status == "succeeded",
            Message = paymentIntent.Status,
            PaymentIntentId = paymentIntent.Id
        };
    }

    public async Task<PaymentResponseDto> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amount.HasValue ? (long)(amount.Value * 100) : null
        };

        var service = new RefundService();
        var refund = await service.CreateAsync(options);

        return new PaymentResponseDto
        {
            Success = refund.Status == "succeeded",
            Message = refund.Status,
            PaymentIntentId = paymentIntentId
        };
    }
}
