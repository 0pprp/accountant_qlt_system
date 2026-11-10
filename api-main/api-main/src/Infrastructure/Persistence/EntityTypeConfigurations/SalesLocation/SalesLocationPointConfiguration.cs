using Domain.Entities.SalesLocationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesLocation;

public class SalesLocationPointConfiguration : IEntityTypeConfiguration<SalesLocationPoint>
{
    public void Configure(EntityTypeBuilder<SalesLocationPoint> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude).IsRequired();
        builder.Property(x => x.Longitude).IsRequired();
        builder.Property(x => x.RecordedAtUtc).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
        builder.Property(x => x.IsLive).IsRequired();
        builder.Property(x => x.ActorKind).IsRequired();
        builder.Property(x => x.ClientEventId).HasMaxLength(SalesLocationPoint.MaxClientEventIdLength);

        builder.HasIndex(x => new { x.ActorKind, x.UserId, x.RecordedAtUtc });
        builder.HasIndex(x => new { x.UserId, x.RecordedAtUtc });
        builder.HasIndex(x => new { x.UserId, x.ClientEventId })
            .IsUnique()
            .HasFilter("\"ClientEventId\" IS NOT NULL");

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
