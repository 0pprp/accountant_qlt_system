using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Product;

public class ProductConfiguration : IEntityTypeConfiguration<Domain.Entities.ProductAggregate.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ProductAggregate.Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RemainingCount).IsRequired();
        builder.Property(x => x.BuyAmount).IsRequired();
        builder.Property(x => x.SellAmount).IsRequired();
        builder.Property(x => x.DailyInstallmentAmount).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(400).IsRequired(false);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}