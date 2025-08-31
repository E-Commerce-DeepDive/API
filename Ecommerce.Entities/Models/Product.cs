using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Entities.Models.Reviews;

namespace Ecommerce.Entities.Models;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
    public int StockQuantity { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    public List<Review> Reviews { get; set; } = new List<Review>();  public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    
}