using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesShift;

public class SalesShiftConfiguration : IEntityTypeConfiguration<Domain.Entities.SalesShiftAggregate.SalesShift>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SalesShiftAggregate.SalesShift> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.EndedAtUtc).IsRequired(false);

        builder.HasIndex(x => new { x.UserId, x.EndedAtUtc });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
