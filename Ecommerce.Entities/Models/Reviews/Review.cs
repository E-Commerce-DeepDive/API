using Ecommerce.Entities.Models.Auth.Identity;
using Ecommerce.Entities.Models.Auth.Users;
using Ecommerce.Utilities.Enums;
using Ecommerce.Utilities.Enums.Reviews;

namespace Ecommerce.Entities.Models.Reviews
{
    public class Review
    {
        public Guid Id { get; set; }
        public string BuyerId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        public double Rating { get; set; } // Overall Rating = average of Attributes
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        
        public bool IsFlagged { get; set; } = false;

        // Navigation Properties
        public Buyer Buyer { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }

        public ICollection<ReviewAttribute> Attributes { get; set; } = new List<ReviewAttribute>();
        public ICollection<ReviewPhoto> Photos { get; set; }
        public ICollection<ReviewHelpfulVote> HelpfulVotes { get; set; }
    }
}
