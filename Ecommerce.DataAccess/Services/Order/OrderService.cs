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
                return new Response<OrderResponse>("User not authenticated", false);

         

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



                var order = CreateOrder(request);

                await _context.Orders.AddAsync(order, cancellationToken);
                UpdateProductStock(request.OrderItems, productsDict);
                await _context.SaveChangesAsync(cancellationToken);


                await transaction.CommitAsync(cancellationToken);



                var response = CreateOrderResponse(order);
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
     
     
      public async Task<Response<PaginatedList<OrderResponse>>> GetAllAsync(string userId, OrderFilters<OrderSorting> filters, CancellationToken cancellationToken)
        {

            if (userId is null)
            {
                _logger.LogWarning("UserId is null while fetching orders");
                return _responseHandler.Unauthorized<PaginatedList<OrderResponse>>("User not authenticated");
            }

            _logger.LogInformation("Fetching orders for user {UserId} with filters: {@Filters}", userId, filters);

            var query = _context.Orders
                .Where(o => o.BuyerId == userId);


            var filteredList = FilteredListItems(query, filters, userId);


            // TODO : Implement search functionality after knowing the criteria
            //        Use (ProjectToType()) after adding Mapster/AutoMapper

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

            _logger.LogInformation("Fetched {Count} orders for buyer {UserId} on page {PageNumber} with page size {PageSize}",
                orders.Items.Count, userId, filters.PageNumber, filters.PageSize);

            return _responseHandler.Success(orders, "Orders fetched successfully");
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
        
        private static Entities.Models.Order CreateOrder(OrderRequest request)
        {
            var order = new Entities.Models.Order
            {
              
                ShippingAddress = request.ShippingAddress,
                ShippingPrice = request.ShippingPrice,
                CourierService = request.CourierService,
                TotalPrice = request.TotalPrice,
                TrackingNumber = request.TrackingNumber,
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
        
        private static OrderResponse CreateOrderResponse(Entities.Models.Order order)
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
                    ProductName = oi.Product.Name,
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