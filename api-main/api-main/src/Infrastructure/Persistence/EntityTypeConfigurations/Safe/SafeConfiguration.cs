using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Safe;

public class SafeConfiguration : IEntityTypeConfiguration<Domain.Entities.SafeAggregate.Safe>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SafeAggregate.Safe> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RemainingCashAmount).IsRequired();
        builder.Property(x => x.DebitAmount).IsRequired();
        builder.Property(x => x.CreditAmount).IsRequired();
        
        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Safe)
            .HasForeignKey<Domain.Entities.SafeAggregate.Safe>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Safe)
            .HasForeignKey(x => x.SafeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}