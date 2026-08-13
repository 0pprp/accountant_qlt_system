using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.ActivityLog;

public class ActivityLogConfiguration : IEntityTypeConfiguration<Domain.Entities.ActivityLogAggregate.ActivityLog>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ActivityLogAggregate.ActivityLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UserRoles).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ActivityType).IsRequired();
        builder.Property(x => x.TargetEntityType).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.Browser).HasMaxLength(100);
        builder.Property(x => x.OperatingSystem).HasMaxLength(100);
        builder.Property(x => x.DeviceType).IsRequired();

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.ActivityLogs)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}