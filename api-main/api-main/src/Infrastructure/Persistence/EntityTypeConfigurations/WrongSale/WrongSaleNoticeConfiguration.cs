using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.WrongSale;

public class WrongSaleNoticeConfiguration : IEntityTypeConfiguration<WrongSaleNotice>
{
    public void Configure(EntityTypeBuilder<WrongSaleNotice> builder)
    {
        builder.ToTable("WrongSaleNotices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(50);
        builder.Property(x => x.FollowerName).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.ReasonBlock).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.SourceApp).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BranchManagerReviewNote).HasMaxLength(1000);
        builder.Property(x => x.ShopStockEstimatedValue).HasPrecision(18, 0);
        builder.Property(x => x.EstimatedDailyRevenue).HasPrecision(18, 0);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.DelegateReadAtUtc, x.CreatedAtUtc });
    }
}
