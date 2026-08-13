using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Order;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Entities.OrderAggregate.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.OrderAggregate.Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BuyAmount).IsRequired();
        builder.Property(x => x.SellAmount).IsRequired();
        builder.Property(x => x.PrepaymentAmount).IsRequired();
        builder.Property(x => x.DailyInstallmentAmount).IsRequired();
        builder.Property(x => x.Location).HasColumnType("jsonb").IsRequired(false);
        builder.Property(x => x.CreationAddress).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.SaleDate).IsRequired(false);
        builder.Property(x => x.SaleTime).IsRequired(false);
        builder.Property(x => x.Step).IsRequired();
        builder.Property(x => x.ExecutionStatus).IsRequired();
        builder.Property(x => x.ApprovalStatus).IsRequired();

        builder.HasOne(x => x.Seller)
            .WithMany(x => x.SoldOrders)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrderList)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.OrderListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InstallmentPayments)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}