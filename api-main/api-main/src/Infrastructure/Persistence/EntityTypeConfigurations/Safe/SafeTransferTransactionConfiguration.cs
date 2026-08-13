using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class SafeTransferTransactionConfiguration : IEntityTypeConfiguration<SafeTransferTransaction>
{
    public void Configure(EntityTypeBuilder<SafeTransferTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
        
        builder.HasOne(x => x.Transaction)
            .WithOne(x => x.SafeTransferTransaction)
            .HasForeignKey<SafeTransferTransaction>(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourceSafe)
            .WithMany()
            .HasForeignKey(x => x.SourceSafeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DestinationSafe)
            .WithMany()
            .HasForeignKey(x => x.DestinationSafeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}