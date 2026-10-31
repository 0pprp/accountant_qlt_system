using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesLocation;

public class SalesLocationPointConfiguration : IEntityTypeConfiguration<Domain.Entities.SalesLocationAggregate.SalesLocationPoint>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SalesLocationAggregate.SalesLocationPoint> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude).IsRequired();
        builder.Property(x => x.Longitude).IsRequired();
        builder.Property(x => x.RecordedAtUtc).IsRequired();
        builder.Property(x => x.IsLive).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.RecordedAtUtc });

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
