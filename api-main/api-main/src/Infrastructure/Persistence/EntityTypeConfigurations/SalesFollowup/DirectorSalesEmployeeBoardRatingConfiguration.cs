using Domain.Entities.SalesFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesFollowup;

public class DirectorSalesEmployeeBoardRatingConfiguration : IEntityTypeConfiguration<DirectorSalesEmployeeBoardRating>
{
    public void Configure(EntityTypeBuilder<DirectorSalesEmployeeBoardRating> builder)
    {
        builder.ToTable("DirectorSalesEmployeeBoardRatings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.RatedByUserName).HasMaxLength(200);
        builder.HasIndex(x => new { x.EmployeeUserId, x.RatedAtUtc });
    }
}
