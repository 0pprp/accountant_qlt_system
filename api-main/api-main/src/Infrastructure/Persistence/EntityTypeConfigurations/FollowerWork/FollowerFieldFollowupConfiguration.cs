using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FollowerWork;

public class FollowerFieldFollowupConfiguration : IEntityTypeConfiguration<FollowerFieldFollowup>
{
    public void Configure(EntityTypeBuilder<FollowerFieldFollowup> builder)
    {
        builder.ToTable("FollowerFieldFollowups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ListName).HasMaxLength(FollowerFieldFollowup.MaxNameLength);
        builder.Property(x => x.CustomerName).HasMaxLength(FollowerFieldFollowup.MaxNameLength);
        builder.Property(x => x.PhoneNumber).HasMaxLength(FollowerFieldFollowup.MaxPhoneLength);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CompletionNote).HasMaxLength(FollowerFieldFollowup.MaxNoteLength);

        builder.HasIndex(x => new { x.FollowerUserId, x.Status })
            .HasDatabaseName("IX_FollowerFieldFollowups_Follower_Status");
        builder.HasIndex(x => new { x.FollowerUserId, x.ListId, x.CustomerId, x.Status })
            .HasDatabaseName("IX_FollowerFieldFollowups_PendingLookup");
        builder.HasIndex(x => new { x.FollowerUserId, x.ListId, x.Status })
            .HasDatabaseName("IX_FollowerFieldFollowups_List_Status");
    }
}
