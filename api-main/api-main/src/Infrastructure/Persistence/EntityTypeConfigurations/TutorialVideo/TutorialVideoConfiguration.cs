using Domain.Entities.TutorialVideoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.TutorialVideo;

public class TutorialVideoConfiguration : IEntityTypeConfiguration<Domain.Entities.TutorialVideoAggregate.TutorialVideo>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TutorialVideoAggregate.TutorialVideo> builder)
    {
        builder.ToTable("TutorialVideos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AppKey).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.QuestionAr).HasMaxLength(400).IsRequired();
        builder.Property(x => x.VideoUrl).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.CreatedByName).HasMaxLength(200);
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.AppKey, x.IsEnabled, x.SortOrder, x.Id });
    }
}
