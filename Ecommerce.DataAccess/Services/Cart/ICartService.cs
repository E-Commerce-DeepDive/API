using Ecommerce.Entities.DTO.Cart;
using Ecommerce.Entities.Shared.Bases;

namespace Ecommerce.DataAccess.Services.Cart
{
    public interface ICartService
    {
        Task<Response<CartResponse>> AddToCartAsync(string buyerId, AddToCartRequest request);
        Task<Response<CartResponse>> GetCartAsync(string buyerId);
        Task<Response<CartResponse>> UpdateCartItemQuantityAsync(string buyerId, UpdateCartItemRequest request);
        Task<Response<CartResponse>> RemoveCartItemAsync(string buyerId, Guid cartItemId);
    }
}
