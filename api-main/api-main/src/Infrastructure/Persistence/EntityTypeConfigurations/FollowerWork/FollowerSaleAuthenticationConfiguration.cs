using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FollowerWork;

public class FollowerSaleAuthenticationConfiguration : IEntityTypeConfiguration<FollowerSaleAuthentication>
{
    public void Configure(EntityTypeBuilder<FollowerSaleAuthentication> builder)
    {
        builder.ToTable("FollowerSaleAuthentications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FollowerName).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.Property(x => x.ShopStockEstimatedValue).HasPrecision(18, 0);
        builder.Property(x => x.EstimatedDailyRevenue).HasPrecision(18, 0);
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => new { x.OrderListId, x.AuthenticatedAtUtc });
        builder.HasIndex(x => new { x.FollowerUserId, x.AuthenticatedAtUtc });
        builder.HasIndex(x => new { x.BranchId, x.OrderListId });
    }
}
