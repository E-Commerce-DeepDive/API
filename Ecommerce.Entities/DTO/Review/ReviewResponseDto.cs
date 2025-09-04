using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Review
{
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public string BuyerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
