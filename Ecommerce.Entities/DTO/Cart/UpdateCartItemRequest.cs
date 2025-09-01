namespace Ecommerce.Entities.DTO.Cart
{
    public class UpdateCartItemRequest
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }

    }
}
