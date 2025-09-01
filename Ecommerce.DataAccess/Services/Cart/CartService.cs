using Ecommerce.DataAccess.ApplicationContext;
//using Ecommerce.DataAccess.Services.Inventory;
using Ecommerce.Entities.DTO.Cart;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared.Bases;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly EcommerceContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<CartService> _logger;
        //private const decimal TaxRate = 0.15m; // 15% tax
        //private const decimal DeliveryFee = 10.00m; // $10 delivery fee
        //private const decimal FreeDeliveryThreshold = 100.00m; // Free delivery for orders over $100

        public CartService(
            EcommerceContext context,
            ResponseHandler responseHandler,
            ILogger<CartService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }
        public async Task<Response<CartResponse>> AddToCartAsync(string buyerId, AddToCartRequest request)
        {
            _logger.LogInformation("Attempting to add product {ProductId} with quantity {Quantity} to cart for BuyerId: {BuyerId}", request.ProductId, request.Quantity, buyerId);

            try
            {
                // REACTOR - TODO : from product to GetProductById()
                // Check if product exists and is it approved and in active status
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted && p.IsActive);
                if (product == null)
                {
                    _logger.LogWarning("Product not found or inactive or not approved from admin. ProductId: {ProductId}", request.ProductId);
                    return _responseHandler.NotFound<CartResponse>("Product not found or inactive and make sure that is approved from admin.");
                }

                // REACTOR - TODO : from "inventory managment"
                // Check if product has enough stock
                if (product.StockQuantity < request.Quantity)
                {
                    _logger.LogWarning("Not enough stock for ProductId: {ProductId}. Requested: {Requested}, Available: {Available}",
                        request.ProductId, request.Quantity, product.StockQuantity);
                    return _responseHandler.BadRequest<CartResponse>("Not enough stock available.");
                }

                // Get or create cart
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.BuyerId == buyerId);
                if (cart == null)
                {
                    cart = new Entities.Models.Cart
                    {
                        Id = Guid.NewGuid(),
                        BuyerId = buyerId,
                        CreatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };
                    _context.Carts.Add(cart);
                }
                if (cart.CartItems == null)
                    cart.CartItems = new List<CartItem>();

                // Check if product already exists in cart
                var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += request.Quantity;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    cart.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = await BuildCartResponseAsync(cart);
                _logger.LogInformation("Product {ProductId} added to cart successfully for BuyerId: {BuyerId}", request.ProductId, buyerId);
                return _responseHandler.Success(response, "Product added to cart successfully.");



            }
            catch (Exception ex)
            {
                {
                    _logger.LogError(ex, "Error occurred while adding product to cart for BuyerId: {BuyerId}", buyerId);
                    return _responseHandler.InternalServerError<CartResponse>("An error occurred while adding product to cart.");
                }
            }
        }
        public async Task<Response<CartResponse>> GetCartAsync(string buyerId)
        {
            _logger.LogInformation("Retrieving cart for BuyerId: {BuyerId}", buyerId);

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images.Where(image => image.IsPrimary))
                    .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    _logger.LogInformation("Cart is empty for BuyerId: {BuyerId}", buyerId);
                    return _responseHandler.Success(new CartResponse
                    {
                        CartId = Guid.Empty,
                        ItemCount = 0,
                        Items = new List<CartItemResponse>()
                    }, "Cart is empty.");
                }

                _logger.LogDebug("Cart contains {ItemCount} items for BuyerId: {BuyerId}", cart.CartItems.Count, buyerId);

                var response = await BuildCartResponseAsync(cart);

                _logger.LogInformation("Cart retrieved successfully for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Success(response, "Cart retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cart for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.InternalServerError<CartResponse>("An error occurred while retrieving cart.");
            }
        }
        public async Task<Response<CartResponse>> UpdateCartItemQuantityAsync(string buyerId, UpdateCartItemRequest request)
        {
            _logger.LogInformation("Attempting to update quantity for CartItemId: {CartItemId} to {Quantity} for BuyerId: {BuyerId}", request.CartItemId, request.Quantity, buyerId);

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

                if (cart == null)
                {
                    _logger.LogWarning("Cart not found for BuyerId: {BuyerId}", buyerId);
                    return _responseHandler.NotFound<CartResponse>("Cart not found.");
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == request.CartItemId);
                if (cartItem == null)
                {
                    _logger.LogWarning("CartItem not found. CartItemId: {CartItemId}", request.CartItemId);
                    return _responseHandler.NotFound<CartResponse>("Cart item not found.");
                }

                cartItem.Quantity = request.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = await BuildCartResponseAsync(cart);
                _logger.LogInformation("Cart item quantity updated successfully for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Success(response, "Cart item quantity updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cart item quantity for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.InternalServerError<CartResponse>("An error occurred while updating cart item quantity.");
            }
        }
        public async Task<Response<CartResponse>> RemoveCartItemAsync(string buyerId, Guid cartItemId)
        {
            _logger.LogInformation("Attempting to remove CartItemId: {CartItemId} for BuyerId: {BuyerId}", cartItemId, buyerId);

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

                if (cart == null)
                {
                    _logger.LogWarning("Cart not found for BuyerId: {BuyerId}", buyerId);
                    return _responseHandler.NotFound<CartResponse>("Cart not found.");
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem == null)
                {
                    _logger.LogWarning("CartItem not found. CartItemId: {CartItemId}", cartItemId);
                    return _responseHandler.NotFound<CartResponse>("Cart item not found.");
                }

                cart.CartItems.Remove(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = await BuildCartResponseAsync(cart);
                _logger.LogInformation("Cart item removed successfully for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.Success(response, "Cart item removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing cart item for BuyerId: {BuyerId}", buyerId);
                return _responseHandler.InternalServerError<CartResponse>("An error occurred while removing cart item.");
            }
        }

        // Helpers Methods
        private async Task<CartResponse> BuildCartResponseAsync(Entities.Models.Cart cart)
        {
            var items = cart.CartItems.Select(ci => new CartItemResponse
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ImageUrl = ci.Product.Images.Where(i => i.IsPrimary).Select(p => p.ImageUrl)?.SingleOrDefault() ?? string.Empty,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price,
                Subtotal = ci.Quantity * ci.Product.Price
            }).ToList();

            // TODO : This is the without tax and the delivery fee (we implment delivery fee in orders)
            decimal summitionOfCartItemsSubTotals = items.Sum(i => i.Subtotal);
            //decimal tax = subtotal * TaxRate;
            //decimal deliveryFee = subtotal > FreeDeliveryThreshold ? 0 : DeliveryFee;
            //decimal total = subtotal + tax + deliveryFee;

            return new CartResponse
            {
                CartId = cart.Id,
                Items = items,
                Subtotal = summitionOfCartItemsSubTotals, // with out any addintions
                //Tax = tax,
                //DeliveryFee = deliveryFee,
                Total = summitionOfCartItemsSubTotals, // after adding tax and delivery 'total'
                ItemCount = items.Sum(i => i.Quantity)
            };
        }


    }
}
