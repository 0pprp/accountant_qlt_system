using Domain.Entities.CommitteeTaskAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.CommitteeTask;

public class CommitteeTaskItemConfiguration : IEntityTypeConfiguration<CommitteeTaskItem>
{
    public void Configure(EntityTypeBuilder<CommitteeTaskItem> builder)
    {
        builder.ToTable("CommitteeTaskItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Body).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.HasIndex(x => new { x.IsEnabled, x.SortOrder, x.Id });
    }
}
