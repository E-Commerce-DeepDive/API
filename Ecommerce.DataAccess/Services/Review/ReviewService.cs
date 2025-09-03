using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.Entities.DTO.Review.Ecommerce.Entities.DTO.Reviews;
using Ecommerce.Entities.DTO.Review;
using Microsoft.EntityFrameworkCore;


namespace Ecommerce.DataAccess.Services.Review
{
    public class ReviewService : IReviewService
    {
        private readonly EcommerceContext _context;

        public ReviewService(EcommerceContext context)
        {
            _context = context;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(ReviewRequestDto dto)
        {
            var review = new Ecommerce.Entities.Models.Reviews.Review
            {
                Id = Guid.NewGuid(),    
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewResponseDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,      
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductAsync(Guid productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    BuyerName = r.Buyer.FirstName,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TopRatedProductDto>> GetTopRatedProductsAsync(int count = 5)
        {
            return await _context.Reviews
                .Where(r => !r.IsDeleted)
                .GroupBy(r => r.ProductId)
                .Select(g => new TopRatedProductDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    AverageRating = g.Average(r => r.Rating),
                    ReviewsCount = g.Count()
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewsCount)
                .Take(count)
                .ToListAsync();
        }
    }
}
