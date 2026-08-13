using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.StatusDescription).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.Direction).IsRequired();

        builder.HasOne(x => x.Safe)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.SafeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.SellerCashDeliveryTransaction)
            .WithOne(x => x.Transaction)
            .HasForeignKey<SellerCashDeliveryTransaction>(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Expense)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Purchase)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SafeTransferTransaction)
            .WithOne(x => x.Transaction)
            .HasForeignKey<SafeTransferTransaction>(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}