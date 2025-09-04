using Ecommerce.Entities.Models.Auth.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Ecommerce.Entities.Models.Reviews;
using Ecommerce.Utilities.Enums;

namespace Ecommerce.Entities.Models.Auth.Users
{
    public class Buyer
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey(nameof(Id))]
        public User User { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public Cart Cart { get; set; } // One-to-One
        public Wishlist Wishlist { get; set; } // One-to-One
        public List<Order> Orders { get; set; } = new List<Order>(); // One-to-Many
        public List<Review> Reviews { get; set; } = new List<Review>(); // One-to-Many
    }
}