using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class SafeConfiguration : IEntityTypeConfiguration<Domain.Entities.SafeAggregate.Safe>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SafeAggregate.Safe> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(SafeNames.MaxLength).IsRequired();
        builder.Property(x => x.Kind).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Balance).IsRequired();
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.Kind })
            .HasDatabaseName("IX_Safes_Branch_Kind");

        builder.HasIndex(x => x.BranchId)
            .IsUnique()
            .HasFilter("\"Kind\" = 0")
            .HasDatabaseName("UX_Safes_BranchSafe");

        builder.HasIndex(x => x.BranchId)
            .IsUnique()
            .HasFilter("\"Kind\" = 1")
            .HasDatabaseName("UX_Safes_ExpensesSafe");

        builder.HasIndex(x => x.OrderListId)
            .IsUnique()
            .HasFilter("\"OrderListId\" IS NOT NULL")
            .HasDatabaseName("UX_Safes_OrderList");

        builder.HasIndex(x => new { x.BranchId, x.UserId })
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL")
            .HasDatabaseName("UX_Safes_Employee");

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Safes)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrderList)
            .WithMany()
            .HasForeignKey(x => x.OrderListId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Movements)
            .WithOne(x => x.Safe)
            .HasForeignKey(x => x.SafeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
