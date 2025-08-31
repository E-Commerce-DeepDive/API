using Ecommerce.Entities.Models.Auth.Identity;

namespace Ecommerce.Entities.Models.Reviews
{
    public class ReviewHelpfulVote
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public string UserId { get; set; }
        public bool IsHelpful { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Review Review { get; set; }
        public User User { get; set; }
    }
}
