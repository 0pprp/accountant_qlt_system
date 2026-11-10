using Domain.Entities.PriceBoardAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.PriceBoard;

public class PriceBoardEntryConfiguration : IEntityTypeConfiguration<PriceBoardEntry>
{
    public void Configure(EntityTypeBuilder<PriceBoardEntry> builder)
    {
        builder.ToTable("PriceBoardEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductName).HasMaxLength(400).IsRequired();
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.DailyInstallment).HasPrecision(18, 2);
        builder.Property(x => x.DownPayment).HasPrecision(18, 2);
        builder.Property(x => x.TargetProvinceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedByName).HasMaxLength(200);
        builder.Property(x => x.UpdatedByName).HasMaxLength(200);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.TargetProvinceId, x.UpdatedAtUtc, x.Id });
        builder.HasIndex(x => new { x.UpdatedAtUtc, x.Id });
    }
}
