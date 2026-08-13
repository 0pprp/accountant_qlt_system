using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.InstallmentPayment;

public class InstallmentPaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(400).IsRequired(false);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.InstallmentPayments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}