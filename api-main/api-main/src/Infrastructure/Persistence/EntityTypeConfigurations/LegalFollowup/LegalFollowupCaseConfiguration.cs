using Domain.Entities.LegalFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.LegalFollowup;

public class LegalFollowupCaseConfiguration : IEntityTypeConfiguration<LegalFollowupCase>
{
    public void Configure(EntityTypeBuilder<LegalFollowupCase> builder)
    {
        builder.ToTable("LegalFollowupCases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(50);
        builder.Property(x => x.CustomerAddress).HasMaxLength(500);
        builder.Property(x => x.FollowerName).HasMaxLength(200);
        builder.Property(x => x.CreatedByName).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.Property(x => x.CompletionNote).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ShopLatitude);
        builder.Property(x => x.ShopLongitude);
        builder.HasIndex(x => new { x.FollowerUserId, x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.SalesRequestId, x.Status });
        builder.HasIndex(x => x.CustomerId);
    }
}
