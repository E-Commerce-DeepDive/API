using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Wishlist
{
    public class WishlistProductDto // it a list element of WishlistViewResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
    }
}
