namespace Ecommerce.Entities.DTO.Order;
public class OrderResponse
{
    public Guid Id { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingPrice { get; set; }
    public string CourierService { get; set; } = string.Empty;
    public decimal TotalPrice => OrderItems.Sum(item => item.Subtotal) + ShippingPrice;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public IList<OrderItemResponse> OrderItems { get; set; } = [];
}
