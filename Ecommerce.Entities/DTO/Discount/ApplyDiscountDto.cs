using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Discount
{
    public class ApplyDiscountDto
    {
        public string Code { get; set; }
        public decimal CartTotal { get; set; }
        public List<Guid> ProductIds { get; set; } 
        public List<Guid> CategoryIds { get; set; } 
    }
}
