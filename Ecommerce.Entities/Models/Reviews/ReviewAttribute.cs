using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Utilities.Enums.Reviews;

namespace Ecommerce.Entities.Models.Reviews
{
    public class ReviewAttribute
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public ReviewAttributeType Aspect { get; set; }
        public int Score { get; set; }

        // Navigation
        public Review Review { get; set; }
    }
}
