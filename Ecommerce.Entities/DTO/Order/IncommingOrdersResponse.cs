namespace Ecommerce.Entities.DTO.Order;
public class IncommingOrdersResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public IEnumerable<OrderItemResponse> OrderItems { get; set; }
    public BuyerDetailsResponse Buyer { get; set; }
}