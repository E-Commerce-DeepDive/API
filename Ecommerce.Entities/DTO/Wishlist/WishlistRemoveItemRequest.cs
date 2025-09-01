using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.DTO.Wishlist
{
    public class WishlistRemoveItemRequest
    {
        public Guid ProductId { get; set; }
    }
}
