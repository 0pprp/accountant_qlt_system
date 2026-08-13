using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.OrderList;

public class OrderListConfiguration : IEntityTypeConfiguration<Domain.Entities.OrderListAggregate.OrderList>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.OrderListAggregate.OrderList> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.Mandob)
            .WithOne(x => x.OrderListAsMandob)
            .HasForeignKey<Domain.Entities.OrderListAggregate.OrderList>(x => x.MandobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Motaba)
            .WithMany(x => x.OrderListsAsMotaba)
            .HasForeignKey(x => x.MotabaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.OrderLists)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}