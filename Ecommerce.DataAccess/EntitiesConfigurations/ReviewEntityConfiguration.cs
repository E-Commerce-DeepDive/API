using Ecommerce.Entities.Models.Reviews;
using Ecommerce.Utilities.Enums.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.DataAccess.EntitiesConfigurations
{
    public class ReviewEntityConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.BuyerId)
                   .IsRequired();
            builder.Property(r => r.OrderId)
                   .IsRequired();
            builder.Property(r => r.ProductId)
                   .IsRequired();
            builder.Property(r => r.Rating)
                   .IsRequired();
            builder.Property(r => r.Comment)
                   .HasMaxLength(1000);
            builder.Property(r => r.IsDeleted)
                   .HasDefaultValue(false);
            builder.Property(r => r.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(ReviewStatus.Pending)
                    .IsRequired();
            builder.Property(r => r.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.UpdatedAt)
                   .IsRequired(false);

            builder.HasOne(r => r.Buyer)
                   .WithMany(b => b.Reviews)
                   .HasForeignKey(r => r.BuyerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Order)
                   .WithMany()
                   .HasForeignKey(r => r.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Product)
                   .WithMany(p => p.Reviews)
                   .HasForeignKey(r => r.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Photos)
                   .WithOne(p => p.Review)
                   .HasForeignKey(p => p.ReviewId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.HelpfulVotes)
                   .WithOne(hv => hv.Review)
                   .HasForeignKey(hv => hv.ReviewId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => new { r.BuyerId, r.ProductId, r.OrderId });


        }
    }
}
