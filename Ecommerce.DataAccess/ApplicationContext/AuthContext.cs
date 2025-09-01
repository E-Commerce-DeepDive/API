using Ecommerce.DataAccess.EntitiesConfigurations;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Models.Auth.Identity;
using Ecommerce.Entities.Models.Auth.Users;
using Ecommerce.Entities.Models.Auth.UserTokens;
using Ecommerce.Entities.Models.Reviews;
using Ecommerce.Utilities.Enums;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.DataAccess.ApplicationContext
{
    public class EcommerceContext : IdentityDbContext<User, Role, string>, IDataProtectionKeyContext
    {
        public EcommerceContext(DbContextOptions<EcommerceContext> options)
             : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new BuyerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProductEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ProductImageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new OrderEntityConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new CartEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CartItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WishlistEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WishlistItemEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewEntityConfiguration());
        }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewAttribute> ReviewAttributes { get; set; }
        public DbSet<ReviewPhoto> ReviewPhotos { get; set; }
        public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
    }
}
