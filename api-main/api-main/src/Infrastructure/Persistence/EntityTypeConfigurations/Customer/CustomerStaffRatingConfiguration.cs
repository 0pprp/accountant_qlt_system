using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Customer;

public class CustomerStaffRatingConfiguration : IEntityTypeConfiguration<CustomerStaffRating>
{
    public void Configure(EntityTypeBuilder<CustomerStaffRating> builder)
    {
        builder.ToTable("CustomerStaffRatings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(CustomerStaffRating.MaxNoteLength);
        builder.Property(x => x.RatedByName).HasMaxLength(CustomerStaffRating.MaxNameLength);

        builder.HasIndex(x => new { x.CustomerId, x.Role }).IsUnique();
        builder.HasIndex(x => x.RatedAtUtc);
    }
}
