using Ecommerce.Entities.DTO.Order;
using Ecommerce.Entities.DTO.Shared;
using Ecommerce.Entities.Shared;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Utilities.Enums;

namespace Ecommerce.DataAccess.Services.Order;

public interface IOrderService
{
    Task<Response<OrderResponse>> AddOrderAsync(OrderRequest request, string userId, CancellationToken cancellationToken); 
    Task<Response<PaginatedList<OrderResponse>>> GetAllAsync(string userId, OrderFilters<OrderSorting> filters, CancellationToken cancellationToken);
    
}