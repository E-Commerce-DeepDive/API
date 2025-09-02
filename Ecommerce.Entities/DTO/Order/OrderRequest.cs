namespace Ecommerce.Entities.DTO.Order;

public class OrderRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingPrice { get; set; }
    public string CourierService { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public decimal TotalPrice => OrderItems.Sum(o => o.Subtotal) + ShippingPrice;
    public List<OrderItemRequest> OrderItems { get; set; } = new List<OrderItemRequest>();

}