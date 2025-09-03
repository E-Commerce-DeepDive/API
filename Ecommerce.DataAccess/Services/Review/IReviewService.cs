using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Entities.DTO.Review.Ecommerce.Entities.DTO.Reviews;
using Ecommerce.Entities.DTO.Review;

namespace Ecommerce.DataAccess.Services.Review
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> AddReviewAsync(ReviewRequestDto dto);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductAsync(Guid productId);
        Task<IEnumerable<TopRatedProductDto>> GetTopRatedProductsAsync(int count = 5);
    }
}
