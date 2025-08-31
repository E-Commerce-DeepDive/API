namespace Ecommerce.Entities.Models.Reviews
{
    public class ReviewPhoto
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }

        public string PhotoUrl { get; set; }
        public string PublicId { get; set; } = null!;

        // Navigation
        public Review Review { get; set; }
    }
}
