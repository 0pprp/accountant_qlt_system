using Domain.Entities.LegalConversionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.LegalConversion;

public class LegalConversionRequestConfiguration : IEntityTypeConfiguration<LegalConversionRequest>
{
    public void Configure(EntityTypeBuilder<LegalConversionRequest> builder)
    {
        builder.ToTable("LegalConversionRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.RequestNote).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.RequesterName).HasMaxLength(200);
        builder.Property(x => x.DecidedByName).HasMaxLength(200);
        builder.Property(x => x.DecisionNote).HasMaxLength(1000);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc, x.Id });
        builder.HasIndex(x => new { x.CustomerId, x.Status });
        builder.HasIndex(x => x.BranchId);
    }
}
