using Ecommerce.Entities.Models;
using Ecommerce.Entities.Models.Auth.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.DataAccess.EntitiesConfigurations
{
    public class BuyerEntityConfiguration : IEntityTypeConfiguration<Buyer>
    {
        public void Configure(EntityTypeBuilder<Buyer> builder)
        {
            // Primary Key
            builder.HasKey(b => b.Id);

            // Properties
            builder.Property(b => b.FirstName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(b => b.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(b => b.BirthDate)
                   .IsRequired();

            builder.Property(b => b.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(b => b.UpdatedAt)
                   .IsRequired(false);

            // One-to-One: Buyer - Cart
            builder.HasOne(b => b.Cart)
                   .WithOne(c => c.Buyer)
                   .HasForeignKey<Cart>(c => c.BuyerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: Buyer - User (Shared PK)
            builder.HasOne(b => b.User)
                   .WithOne()
                   .HasForeignKey<Buyer>(b => b.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: Buyer - Wishlist
            builder.HasOne(b => b.Wishlist)
                   .WithOne(w => w.Buyer)
                   .HasForeignKey<Wishlist>(w => w.BuyerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Buyer - Orders
            builder.HasMany(b => b.Orders)
                   .WithOne(o => o.Buyer)
                   .HasForeignKey(o => o.BuyerId);

            // One-to-Many: Buyer - Reviews
            builder.HasMany(b => b.Reviews)
                   .WithOne(r => r.Buyer)
                   .HasForeignKey(r => r.BuyerId);
        }
    }
}
