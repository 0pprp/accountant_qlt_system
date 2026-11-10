using Domain.Entities.SalesFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesFollowup;

public class SalesEmployeeAppBlockConfiguration : IEntityTypeConfiguration<SalesEmployeeAppBlock>
{
    public void Configure(EntityTypeBuilder<SalesEmployeeAppBlock> builder)
    {
        builder.ToTable("SalesEmployeeAppBlocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.HasIndex(x => x.EmployeeUserId).IsUnique();
    }
}
