using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FollowerWork;

public class FollowerCustomerRatingSnapshotConfiguration
    : IEntityTypeConfiguration<FollowerCustomerRatingSnapshot>
{
    public void Configure(EntityTypeBuilder<FollowerCustomerRatingSnapshot> builder)
    {
        builder.ToTable("FollowerCustomerRatingSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerName).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.RatingLabel).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.CustomerId, x.CaptureDate }).IsUnique();
        builder.HasIndex(x => new { x.OrderListId, x.CaptureDate });
    }
}
