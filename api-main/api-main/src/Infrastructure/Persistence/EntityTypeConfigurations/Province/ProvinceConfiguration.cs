using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Province;

public class ProvinceConfiguration : IEntityTypeConfiguration<Domain.Entities.ProvinceAggregate.Province>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ProvinceAggregate.Province> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasMany(x => x.Branches)
            .WithOne(x => x.Province)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}