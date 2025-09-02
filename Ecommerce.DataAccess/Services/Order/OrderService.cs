using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.Entities.DTO.Order;
using Ecommerce.Entities.DTO.Shared;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Utilities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Order;

public class OrderService : IOrderService
{
    private readonly EcommerceContext _context;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<OrderService> _logger;

    public OrderService(EcommerceContext context , ResponseHandler responseHandler,ILogger<OrderService> logger)
    {
        _context =  context;
        _responseHandler = responseHandler;
        _logger = logger;
    }
    
     public async Task<Response<OrderResponse>> AddOrderAsync(OrderRequest request, string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                return _responseHandler.Unauthorized<OrderResponse>("User not authenticated");
            
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var productIds = request.OrderItems.Select(oi => oi.ProductId).ToHashSet();

                var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync(cancellationToken);


                if (products.Count != productIds.Count)
                {
                    var foundIds = products.Select(p => p.Id).ToHashSet();
                    var missingProducts = productIds.Where(id => !foundIds.Contains(id)).ToList();
                    return _responseHandler.BadRequest<OrderResponse>($"Products not found: {string.Join(", ", missingProducts)}");

                }



                var productsDict = products.ToDictionary(p => p.Id);

                var businessRuleErrors = ValidateBusinessRules(request.OrderItems, productsDict);

                if (businessRuleErrors.Any())
                {
                    _logger.LogWarning("Business rule validation failed for user {UserId}: {ValidationErrors}",
                        userId, string.Join("; ", businessRuleErrors));
                    return _responseHandler.BadRequest<OrderResponse>(
                        $"Order validation failed: {string.Join("; ", businessRuleErrors)}");
                }



                var order = CreateOrder(request, userId);

                await _context.Orders.AddAsync(order, cancellationToken);
                UpdateProductStock(request.OrderItems, productsDict);
                if (productsDict.Values.Any(p => p.StockQuantity < 0))
                {
                    _logger.LogWarning("Insufficient stock detected after attempting to reserve items for user {UserId}", userId);
                    await transaction.RollbackAsync(cancellationToken);
                    return _responseHandler.BadRequest<OrderResponse>("Insufficient stock for one or more items");
                }
                await _context.SaveChangesAsync(cancellationToken);


                await transaction.CommitAsync(cancellationToken);



                var response = CreateOrderResponse(order,productsDict);
                _logger.LogInformation(
                    "Order placed successfully - OrderId: {OrderId}, UserId: {UserId}, Items: {ItemCount}, Total: ${TotalPrice:F2}, Shipping: {ShippingService} to {ShippingAddress}",
                    order.Id,
                    userId,
                    order.OrderItems.Count,
                    order.TotalPrice,
                    order.CourierService,
                    order.ShippingAddress);
                return _responseHandler.Success(response, "order placed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while adding order for user {UserId}", userId);
                return _responseHandler.InternalServerError<OrderResponse>(ex.Message);
            }

        }
     
     
        public async Task<Response<PaginatedList<OrderResponse>>> GetAllAsync(string? userId, OrderFilters<OrderSorting> filters, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching orders for user {UserId} with filters: {@Filters}", userId ?? "ALL", filters);

            IQueryable<Entities.Models.Order> query = _context.Orders.Where(o => !o.IsDeleted);
            
            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(o => o.BuyerId == userId);
            }

            var filteredList = FilteredListItems(query, filters, userId);

            var source = filteredList.Include(o => o.OrderItems)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    BuyerId = o.BuyerId,
                    ShippingAddress = o.ShippingAddress,
                    ShippingPrice = o.ShippingPrice,
                    CourierService = o.CourierService,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                    }).ToList()
                })
                .AsNoTracking().AsQueryable();

            var orders = await PaginatedList<OrderResponse>.CreateAsync(source, filters.PageNumber, filters.PageSize, cancellationToken);

            _logger.LogInformation("Fetched {Count} orders for user {UserId} on page {PageNumber} with page size {PageSize}",
                orders.Items.Count, userId ?? "ALL", filters.PageNumber, filters.PageSize);

            return _responseHandler.Success(orders, "Orders fetched successfully");
        }

        public async Task<Response<bool>> UpdateOrderAsync(string orderId, UpdateOrderRequest request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
            {
                _logger.LogWarning("Invalid order ID format: {OrderId}", orderId);
                return _responseHandler.BadRequest<bool>("Invalid order ID format");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderGuid && !o.IsDeleted, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                return _responseHandler.NotFound<bool>($"Order with ID {orderId} not found");
            }

            if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            {
                _logger.LogWarning("Order with ID {OrderId} cannot be updated because it is in status {OrderStatus}", orderId, order.Status);
                return _responseHandler.BadRequest<bool>($"Order with status {order.Status} cannot be updated");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                order.Status = request.Status;
                order.ShippingAddress = request.ShippingAddress;
                order.CourierService = request.CourierService;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Order {OrderId} updated successfully", orderId);
                await transaction.CommitAsync(cancellationToken);
                return _responseHandler.Success(true, "Order updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while updating order {OrderId}", orderId);
                return _responseHandler.InternalServerError<bool>("An error occurred while updating the order");
            }
        }

      public async Task<Response<bool>> ConfirmOrderAsync(string orderId, CancellationToken cancellationToken)
{
    if (!Guid.TryParse(orderId, out var orderGuid))
    {
        _logger.LogWarning("Invalid order ID format: {OrderId}", orderId);
        return _responseHandler.BadRequest<bool>("Invalid order ID format");
    }

    var order = await _context.Orders
        .FirstOrDefaultAsync(o => o.Id == orderGuid && !o.IsDeleted, cancellationToken);

    if (order is null)
    {
        _logger.LogWarning("Order with ID {OrderId} not found", orderId);
        return _responseHandler.NotFound<bool>($"Order with ID {orderId} not found");
    }

    if (order.Status == OrderStatus.Confirmed)
    {
        _logger.LogWarning("Order with ID {OrderId} is already confirmed", orderId);
        return _responseHandler.BadRequest<bool>($"Order is already confirmed");
    }

    if (order.Status != OrderStatus.Pending)
    {
        _logger.LogWarning("Order with ID {OrderId} and status {OrderStatus} cannot be confirmed unless it is pending", orderId, order.Status);
        return _responseHandler.BadRequest<bool>($"Order with status {order.Status} cannot be confirmed");
    }

    _logger.LogInformation("Confirming order {OrderId}", orderId);

    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} confirmed successfully", orderId);
        await transaction.CommitAsync(cancellationToken);
        return _responseHandler.Success(true, "Order confirmed successfully");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Error occurred while confirming order {OrderId}", orderId);
        return _responseHandler.InternalServerError<bool>("An error occurred while confirming the order");
    }
}

       
       public async Task<Response<bool>> CancelOrderAsync(string orderId, string userId, bool isAdmin, AdminCancelOrderRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User with id {UserId} is trying to canecel order with id {OrderId}", userId, orderId);

            if (!Guid.TryParse(orderId, out var orderGuid))
            {
                _logger.LogWarning("Invalid order ID format: {OrderId}", orderId);
                return _responseHandler.BadRequest<bool>("Invalid order ID format");
            }

            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var order = await _context.Orders
                 .Where(o => o.Id == orderGuid && !o.IsDeleted)
                 .Include(o => o.OrderItems)
                 .SingleOrDefaultAsync(cancellationToken);

                if (order is null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for user {UserId}", orderId, userId);
                    return _responseHandler.NotFound<bool>($"Order with ID {orderId} not found for user {userId}");
                }


                if (!isAdmin && order.BuyerId != userId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to cancel order {OrderId}", userId, orderId);
                    return _responseHandler.Unauthorized<bool>("You are not authorized to cancel this order");
                }

                if (isAdmin && string.IsNullOrWhiteSpace(request.CancelationReason))
                {
                    _logger.LogInformation("Admin user {UserId} tried to cancel order {OrderId} without reason", userId, orderId);
                    return _responseHandler.BadRequest<bool>("Admin must provide a reason for cancellation");
                }

                var cancellableStatuses = isAdmin 
                    ? new[] { OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Pending } 
                    : new[] { OrderStatus.Pending };
                if (!cancellableStatuses.Contains(order.Status))
                {
                    _logger.LogWarning("Order with ID {OrderId} with status {OrderStatus} cannot be cancelled for user {UserId}", orderId, order.Status, userId);
                    return _responseHandler.BadRequest<bool>($"Order with status {order.Status} cannot be cancelled");
                }

                order.Status = OrderStatus.Cancelled;
                order.CancelledDate = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                // Dependancy: Inventory management feature - update stock quantities
                var productIds = order.OrderItems.Select(oi => oi.ProductId);
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync(cancellationToken);

                var productsDict = products.ToDictionary(p => p.Id);

                foreach (var item in order.OrderItems)
                {
                    if (productsDict.TryGetValue(item.ProductId, out var product))
                    {
                        product.StockQuantity += item.Quantity;
                    }
                    else
                    {
                        _logger.LogWarning("Product with ID {ProductId} not found while restoring stock for cancelled order {OrderId}", item.ProductId, orderId);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Order with ID {OrderId} cancelled successfully for user {UserId} - Reason: {Reason}", orderId, userId, request.CancelationReason);

                return _responseHandler.Success(true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while cancelling order with ID {OrderId} for user {UserId}", orderId, userId);
                return _responseHandler.InternalServerError<bool>("An error occurred while cancelling the order");
            }
        }
       
       
        public async Task<Response<bool>> DeleteOrderAsync(string orderId, string userId, bool isAdmin, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
                return _responseHandler.BadRequest<bool>("Invalid order id format");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderGuid && !o.IsDeleted, cancellationToken);
            if (order == null)
                return _responseHandler.NotFound<bool>("Order not found");

            // Buyer can delete only if Cancelled
            if (!isAdmin)
            {
                if (order.BuyerId != userId)
                    return _responseHandler.Unauthorized<bool>("You are not authorized to delete this order");
                if (order.Status != OrderStatus.Cancelled)
                    return _responseHandler.BadRequest<bool>("Only cancelled orders can be deleted by buyer");
            }

            order.IsDeleted = true;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return _responseHandler.Success(true, "Order deleted successfully");
        }

        private IQueryable<Entities.Models.Order> FilteredListItems<TSorting>(IQueryable<Entities.Models.Order> query, OrderFilters<TSorting> filters, string? userId)
            where TSorting : struct, Enum
        {
            if (filters.SortColumn is not null)
            {
                query = ApplySorting(query, filters.SortColumn.Value, filters.SortDirection);
                _logger.LogInformation("Sorting orders for user {UserId} by {SortProperty} in {SortDirection} order",
                    userId, filters.SortColumn, filters.SortDirection);
            }
            else
            {
                query = query.OrderByDescending(o => o.OrderDate);
            }
            if (filters.Status is not null)
            {
                query = query.Where(o => o.Status == filters.Status);
                _logger.LogInformation("Filtered orders for user {UserId} by status {Status} ",
                    userId, filters.Status);
            }
            return query;
        }

        private static IQueryable<Entities.Models.Order> ApplySorting<TSorting>(IQueryable<Entities.Models.Order> query, TSorting sortColumn, SortDirection? direction)
        {
            var isAscending = direction == SortDirection.ASC;

            return sortColumn switch
            {
                OrderSorting.OrderDate => isAscending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate),
                OrderSorting.Status => isAscending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
                
                _ => query.OrderByDescending(o => o.OrderDate)
            };
        }
        private static List<string> ValidateBusinessRules(IEnumerable<OrderItemRequest> orderItems, Dictionary<Guid, Entities.Models.Product> productsDict)
        {
            var errors = new List<string>();

            foreach (var orderItem in orderItems)
            {
                if (!productsDict.TryGetValue(orderItem.ProductId, out var product))
                {
                    errors.Add($"Product {orderItem.ProductId} not found");
                    continue;
                }


                // Stock availability validation
                if (product.StockQuantity < orderItem.Quantity)
                {
                    errors.Add($"{product.Name}: Insufficient stock (Available: {product.StockQuantity}, Requested: {orderItem.Quantity})");
                }

                // Price consistency validation 
                if (product.Price != orderItem.UnitPrice)
                {
                    errors.Add($"{product.Name}: Price has changed (Current: {product.Price:C}, Provided: {orderItem.UnitPrice:C})");
                }
            }

            return errors;
        }
        
        private static Entities.Models.Order CreateOrder(OrderRequest request, string buyerId)
        {
            var order = new Entities.Models.Order
            {
                BuyerId = buyerId,
                ShippingAddress = request.ShippingAddress,
                ShippingPrice = request.ShippingPrice,
                CourierService = request.CourierService,
                TotalPrice = request.TotalPrice,
                TrackingNumber = request.TrackingNumber,
                Status = OrderStatus.Pending, 
                OrderDate = DateTime.UtcNow,
                OrderItems = request.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Subtotal = item.Subtotal,
                    UnitPrice = item.UnitPrice,
                }).ToList()
            };

            return order;
        }

        
        private static OrderResponse CreateOrderResponse(Entities.Models.Order order, Dictionary<Guid, Entities.Models.Product> productsDict)
        {
            return new OrderResponse
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                ShippingAddress = order.ShippingAddress,
                ShippingPrice = order.ShippingPrice,
                CourierService = order.CourierService,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = productsDict.TryGetValue(oi.ProductId, out var prod) ? prod.Name : "Unknown"
                }).ToList()
            };
        }

        
        private static void UpdateProductStock(IEnumerable<OrderItemRequest> orderItems, Dictionary<Guid, Entities.Models.Product> productsDict)
        {
            foreach (var orderItem in orderItems)
            {
                if (productsDict.TryGetValue(orderItem.ProductId, out var product))
                {
                    product.StockQuantity -= orderItem.Quantity;
                }
            }
        }

}