using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Notification;

public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.NotificationAggregate.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.NotificationAggregate.Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ActionType).IsRequired();
        builder.Property(x => x.HasRead).IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.HasRead, x.CreatedAt });

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
