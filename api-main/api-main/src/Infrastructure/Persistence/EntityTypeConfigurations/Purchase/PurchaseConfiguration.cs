using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Purchase;

public class PurchaseConfiguration : IEntityTypeConfiguration<Domain.Entities.PurchaseAggregate.Purchase>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PurchaseAggregate.Purchase> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FactorNumber).IsRequired();
        builder.Property(x => x.TotalAmount).IsRequired();
        builder.Property(x => x.SafeType).IsRequired();

        builder.HasMany(x => x.PurchaseItems)
            .WithOne(x => x.Purchase)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Purchases)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Order)
            .WithOne()
            .HasForeignKey<Domain.Entities.PurchaseAggregate.Purchase>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Purchase)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}