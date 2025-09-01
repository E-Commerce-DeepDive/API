using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.Entities.DTO.Wishlist;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Wishlist
{
    public class WishlistService : IWishlistService
    {
        private readonly EcommerceContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(
            EcommerceContext context,
            ResponseHandler responseHandler,
            ILogger<WishlistService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<WishlistViewResponse>> ViewWishlistAsync(string buyerId)
        {
            _logger.LogInformation("Starting ViewWishlistAsync for BuyerId: {BuyerId}", buyerId);

            var projectedWishlist = await _context.Wishlists
                .Where(w => w.BuyerId == buyerId)
                .Select(w => new WishlistViewResponse
                {
                    Products = w.WishlistItems.Select(item => new WishlistProductDto
                    {
                        ProductId = item.Product.Id,
                        ProductName = item.Product.Name,
                        ProductImageUrl = item.Product.Images
                                            .Where(i => i.IsPrimary)
                                            .Select(i => i.ImageUrl)
                                            .FirstOrDefault(),
                        Price = item.Product.Price,
                        InStock = item.Product.StockQuantity > 0
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (projectedWishlist == null || !projectedWishlist.Products.Any())
            {
                _logger.LogInformation("No wishlist items found for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Success(new WishlistViewResponse(), "No wishlist items found.");
            }

            _logger.LogInformation("Wishlist retrieved successfully for BuyerId: {BuyerId}", buyerId);
            return _responseHandler.Success(projectedWishlist, "Wishlist retrieved successfully.");
        }

        public async Task<Response<string>> AddToWishlistAsync(string buyerId, WishlistAddItemRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Starting AddToWishlistAsync for buyerId: {BuyerId}, productId: {ProductId}", buyerId, request.ProductId);

            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId); // method in product service
                if (product == null || !product.IsActive || product.IsDeleted) // method in product service  
                {
                    _logger.LogWarning("Invalid product. ProductId: {ProductId}", request.ProductId);
                    return _responseHandler.NotFound<string>("The selected product does not exist or is no longer available.");
                }

                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.BuyerId == buyerId);

                var isNewWishlist = false;

                if (wishlist == null)
                {
                    _logger.LogInformation("No wishlist found for BuyerId: {BuyerId}. Creating new wishlist.", buyerId);

                    wishlist = new Entities.Models.Wishlist
                    {
                        Id = Guid.NewGuid(),
                        BuyerId = buyerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    isNewWishlist = true;
                }

                var exists = wishlist.WishlistItems.Any(wi => wi.ProductId == request.ProductId);
                if (exists)
                {
                    _logger.LogWarning("Product already exists in wishlist. BuyerId: {BuyerId}, ProductId: {ProductId}", buyerId, request.ProductId);
                    return _responseHandler.BadRequest<string>("Product already in wishlist.");
                }

                var wishlistItem = new WishlistItem
                {
                    Id = Guid.NewGuid(),
                    WishlistId = wishlist.Id,
                    ProductId = request.ProductId,
                    CreatedAt = DateTime.UtcNow
                };

                if (isNewWishlist)
                {
                    await _context.Wishlists.AddAsync(wishlist);
                    await _context.WishlistItems.AddAsync(wishlistItem);
                }
                else
                {
                    _context.WishlistItems.Add(wishlistItem);
                }

                _logger.LogInformation("Saving wishlist and item to database...");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("AddToWishlistAsync completed successfully. BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Created("Product added to wishlist successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "AddToWishlistAsync error. BuyerId: {BuyerId}", buyerId);
                return _responseHandler.BadRequest<string>("An error occurred while adding to wishlist.");
            }
        }


        public async Task<Response<string>> RemoveFromWishlistAsync(string buyerId, WishlistRemoveItemRequest request)
        {
            _logger.LogInformation("Starting RemoveFromWishlistAsync. BuyerId: {BuyerId}, ProductId: {ProductId}", buyerId, request.ProductId);

            try
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.BuyerId == buyerId);

                if (wishlist == null)
                {
                    _logger.LogWarning("Wishlist not found for BuyerId: {BuyerId}", buyerId);
                    return _responseHandler.NotFound<string>("Wishlist not found.");
                }

                var item = wishlist.WishlistItems.FirstOrDefault(w => w.ProductId == request.ProductId);
                if (item == null)
                {
                    _logger.LogWarning("Product not in wishlist. BuyerId: {BuyerId}, ProductId: {ProductId}", buyerId, request.ProductId);
                    return _responseHandler.NotFound<string>("Product not in wishlist.");
                }

                _context.WishlistItems.Remove(item);

                _logger.LogInformation("Product removed from wishlist in memory. Saving changes...");

                await _context.SaveChangesAsync();

                _logger.LogInformation("RemoveFromWishlistAsync completed. BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Deleted<string>("Product removed from wishlist.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveFromWishlistAsync error. BuyerId: {BuyerId}", buyerId);
                return _responseHandler.BadRequest<string>("An error occurred while removing from wishlist.");
            }
        }
    }
}
