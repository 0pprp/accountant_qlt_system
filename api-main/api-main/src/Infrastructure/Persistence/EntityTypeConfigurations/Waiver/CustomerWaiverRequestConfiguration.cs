using Domain.Entities.WaiverAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Waiver;

public class CustomerWaiverRequestConfiguration : IEntityTypeConfiguration<CustomerWaiverRequest>
{
    public void Configure(EntityTypeBuilder<CustomerWaiverRequest> builder)
    {
        builder.ToTable("CustomerWaiverRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.RequestNote).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.RequesterName).HasMaxLength(200);
        builder.Property(x => x.DecidedByName).HasMaxLength(200);
        builder.Property(x => x.DecisionNote).HasMaxLength(500);
        builder.Property(x => x.AppliedByName).HasMaxLength(200);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc, x.Id });
        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.CustomerId);
    }
}
