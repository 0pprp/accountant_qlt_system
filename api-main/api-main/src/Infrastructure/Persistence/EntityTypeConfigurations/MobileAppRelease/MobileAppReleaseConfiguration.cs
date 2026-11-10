using Domain.Entities.MobileAppReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.MobileAppRelease;

public class MobileAppReleaseConfiguration : IEntityTypeConfiguration<Domain.Entities.MobileAppReleaseAggregate.MobileAppRelease>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.MobileAppReleaseAggregate.MobileAppRelease> builder)
    {
        builder.ToTable("MobileAppReleases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AppKey).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.AppKey).IsUnique();
        builder.Property(x => x.LatestVersionName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ApkUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(128);
        builder.Property(x => x.ReleaseNotes).HasMaxLength(4000);
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
    }
}
