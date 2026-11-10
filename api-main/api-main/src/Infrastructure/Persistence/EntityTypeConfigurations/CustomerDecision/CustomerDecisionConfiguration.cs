using Domain.Entities.CustomerDecisionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.CustomerDecisions;

public class CustomerDecisionConfiguration : IEntityTypeConfiguration<CustomerDecision>
{
    public void Configure(EntityTypeBuilder<CustomerDecision> builder)
    {
        builder.ToTable("CustomerDecisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DecisionType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Note).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ActorName).HasMaxLength(200);
        builder.HasIndex(x => new { x.BranchId, x.CreatedAtUtc, x.Id });
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAtUtc });
        builder.HasIndex(x => x.DecisionType);
    }
}
