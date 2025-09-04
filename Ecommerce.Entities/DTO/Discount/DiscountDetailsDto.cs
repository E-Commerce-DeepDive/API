using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Utilities.Enums;

namespace Ecommerce.Entities.DTO.Discount
{
    public class DiscountDetailsDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public List<Guid> ApplicableProductIds { get; set; }
        public List<Guid> ApplicableCategoryIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public string Status { get; set; }
    }
}
