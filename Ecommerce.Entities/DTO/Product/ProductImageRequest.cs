namespace Ecommerce.Entities.DTO.Product;

public class ProductImageRequest
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UpdatedAt { get; set; }
}