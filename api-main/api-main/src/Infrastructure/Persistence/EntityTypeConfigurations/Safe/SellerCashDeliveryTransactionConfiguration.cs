using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class SellerCashDeliveryTransactionConfiguration : IEntityTypeConfiguration<SellerCashDeliveryTransaction>
{
    public void Configure(EntityTypeBuilder<SellerCashDeliveryTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.Date).IsRequired(false);

        builder.HasOne(x => x.Seller)
            .WithMany(x => x.SellerCashDeliveryTransactions)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Transaction)
            .WithOne(x => x.SellerCashDeliveryTransaction)
            .HasForeignKey<SellerCashDeliveryTransaction>(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}