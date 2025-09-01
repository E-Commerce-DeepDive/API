using Ecommerce.Entities.DTO.Wishlist;

using Ecommerce.Entities.Shared.Bases;

namespace Ecommerce.DataAccess.Services.Wishlist
{
    public interface IWishlistService
    {
        Task<Entities.Shared.Bases.Response<WishlistViewResponse>> ViewWishlistAsync(string buyerId);
        Task<Entities.Shared.Bases.Response<string>> AddToWishlistAsync(string buyerId, WishlistAddItemRequest request);
        Task<Entities.Shared.Bases.Response<string>> RemoveFromWishlistAsync(string buyerId, WishlistRemoveItemRequest request);
    }

}
