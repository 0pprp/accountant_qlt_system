using Domain.Entities.DelegateFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.DelegateFollowup;

public class ListSupervisionDayRatingConfiguration : IEntityTypeConfiguration<ListSupervisionDayRating>
{
    public void Configure(EntityTypeBuilder<ListSupervisionDayRating> builder)
    {
        builder.ToTable("ListSupervisionDayRatings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.RatedByUserName).HasMaxLength(200);
        builder.HasIndex(x => new { x.OrderListId, x.RatingDate }).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.RatingDate });

        builder.HasOne<Domain.Entities.OrderListAggregate.OrderList>()
            .WithMany()
            .HasForeignKey(x => x.OrderListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Entities.BranchAggregate.Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
