using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Review
{
    namespace Ecommerce.Entities.DTO.Reviews
    {
        public class ReviewRequestDto
        {
            public Guid ProductId { get; set; }
            public Guid OrderId { get; set; }
            public double Rating { get; set; }
            public string Comment { get; set; }
        }

     
       
    }

}
