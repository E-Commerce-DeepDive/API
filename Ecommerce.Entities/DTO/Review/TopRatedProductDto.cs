using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Review
{
    public class TopRatedProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}
