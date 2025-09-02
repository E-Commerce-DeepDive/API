namespace Ecommerce.Entities.DTO.Order;
public class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
    public decimal UnitPrice { get; set; }

};