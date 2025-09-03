using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Entities.DTO.Payment;

namespace Ecommerce.DataAccess.Services.Payment
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request);
        Task<PaymentResponseDto> ConfirmPaymentAsync(string paymentIntentId);
        Task<PaymentResponseDto> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
    }

}
