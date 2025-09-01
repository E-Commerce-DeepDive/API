using Ecommerce.Utilities.Enums;

namespace Ecommerce.Entities.DTO.Product;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
        public ShippingOptions ShippingOption { get; set; }
    
    public List<string> ImageUrls { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}