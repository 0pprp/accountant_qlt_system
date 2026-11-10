using Domain.Entities.DelegateFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.DelegateFollowup;

public class FollowerDelegateDayRatingConfiguration : IEntityTypeConfiguration<FollowerDelegateDayRating>
{
    public void Configure(EntityTypeBuilder<FollowerDelegateDayRating> builder)
    {
        builder.ToTable("FollowerDelegateDayRatings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => new { x.FollowerUserId, x.OrderListId, x.RatingDate }).IsUnique();
        builder.HasIndex(x => new { x.OrderListId, x.RatingDate });

        builder.HasOne<Domain.Entities.OrderListAggregate.OrderList>()
            .WithMany()
            .HasForeignKey(x => x.OrderListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Entities.BranchAggregate.Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Entities.UserAggregate.User>()
            .WithMany()
            .HasForeignKey(x => x.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
