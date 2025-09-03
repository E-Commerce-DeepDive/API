using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Payment
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }         // المبلغ
        public string Currency { get; set; } = "pound"; // العملة الافتراضية
        public string Description { get; set; }     // وصف العملية
        public string CustomerEmail { get; set; }   // إيميل العميل
    }
}
