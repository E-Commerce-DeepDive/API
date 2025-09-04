using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Entities.DTO.Discount;

namespace Ecommerce.DataAccess.Services.Discount
{
    public interface IDiscountService
    {
        Task<Response<Guid>> CreateDiscountAsync(CreateDiscountDto dto);
        Task<Response<IEnumerable<DiscountDetailsDto>>> GetAllDiscountsAsync(string? status = null);
        Task<Response<DiscountDetailsDto>> GetDiscountByIdAsync(Guid id);
        Task<Response<bool>> UpdateDiscountAsync(UpdateDiscountDto dto);
        Task<Response<bool>> DeleteDiscountAsync(Guid id);

     
        Task<Response<decimal>> ApplyDiscountAsync(string code, decimal cartTotal, List<Guid> productIds, List<Guid> categoryIds);
    }
}

