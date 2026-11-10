using Domain.Entities.SalesEmployeeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesEmployee;

public class SalesEmployeeAssignmentConfiguration : IEntityTypeConfiguration<SalesEmployeeAssignment>
{
    public void Configure(EntityTypeBuilder<SalesEmployeeAssignment> builder)
    {
        builder.ToTable("SalesEmployeeAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.HasIndex(x => new { x.UserId, x.BranchId, x.EndsAtUtc });
        builder.HasIndex(x => new { x.BranchId, x.EndsAtUtc, x.StartsAtUtc });
    }
}
