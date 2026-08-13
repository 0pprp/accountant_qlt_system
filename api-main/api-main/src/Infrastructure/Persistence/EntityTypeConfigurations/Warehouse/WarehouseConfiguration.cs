using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Warehouse;

public class WarehouseConfiguration : IEntityTypeConfiguration<Domain.Entities.WarehouseAggregate.Warehouse>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WarehouseAggregate.Warehouse> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Warehouses)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Warehouse)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}