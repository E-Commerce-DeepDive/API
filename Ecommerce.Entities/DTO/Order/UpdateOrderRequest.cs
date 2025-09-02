using Ecommerce.Utilities.Enums;

namespace Ecommerce.Entities.DTO.Order;
public class UpdateOrderRequest
{
    public OrderStatus Status { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string CourierService { get; set; } = string.Empty;
}
