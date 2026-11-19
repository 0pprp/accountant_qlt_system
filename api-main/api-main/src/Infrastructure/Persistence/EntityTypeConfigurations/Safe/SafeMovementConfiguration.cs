using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class SafeMovementConfiguration : IEntityTypeConfiguration<SafeMovement>
{
    public void Configure(EntityTypeBuilder<SafeMovement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SafeId).IsRequired();
        builder.Property(x => x.Kind).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.BalanceAfter).IsRequired();
        builder.Property(x => x.PerformedByUserId).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(SafeMovement.NoteMaxLength).IsRequired(false);
        builder.Property(x => x.ImportKey).HasMaxLength(SafeMovement.ImportKeyMaxLength).IsRequired(false);

        builder.HasIndex(x => new { x.SafeId, x.Id })
            .HasDatabaseName("IX_SafeMovements_Safe_Id");

        builder.HasIndex(x => x.TransferGroupId)
            .HasFilter("\"TransferGroupId\" IS NOT NULL")
            .HasDatabaseName("IX_SafeMovements_TransferGroup");

        builder.HasIndex(x => x.OperationKey)
            .IsUnique()
            .HasFilter("\"OperationKey\" IS NOT NULL")
            .HasDatabaseName("UX_SafeMovements_OperationKey");

        builder.HasIndex(x => x.ImportKey)
            .IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_SafeMovements_ImportKey");

        builder.HasOne(x => x.Safe)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.SafeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CounterpartySafe)
            .WithMany()
            .HasForeignKey(x => x.CounterpartySafeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
