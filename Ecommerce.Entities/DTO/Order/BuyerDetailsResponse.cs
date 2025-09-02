namespace Ecommerce.Entities.DTO.Order;
public class BuyerDetailsResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;

}
