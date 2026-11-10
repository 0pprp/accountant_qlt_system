using Domain.Entities.AnnouncementAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Announcement;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Domain.Entities.AnnouncementAggregate.Announcement>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.AnnouncementAggregate.Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AppKey).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ContentType).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.TextBody).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.BannerStorageKey).HasMaxLength(260);
        builder.Property(x => x.ZoomStorageKey).HasMaxLength(260);
        builder.Property(x => x.LinkUrl).HasMaxLength(1000);
        builder.Property(x => x.CreatedByName).HasMaxLength(200);
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.AppKey, x.IsEnabled, x.SortOrder, x.Id });
        builder.HasIndex(x => new { x.UpdatedAtUtc, x.Id });
    }
}
