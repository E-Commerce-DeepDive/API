namespace Ecommerce.Entities.DTO.Cart
{
    public class CartResponse
    {
        public Guid CartId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        public decimal Subtotal { get; set; }
        //public decimal Tax { get; set; }
        //public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
    }
}
