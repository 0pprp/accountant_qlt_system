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
        builder.Property(x => x.ClientPaymentId);
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.Property(x => x.OrderId).IsRequired(false);
        builder.Property(x => x.CustomerId).IsRequired(false);

        builder.HasIndex(x => x.ClientPaymentId)
            .IsUnique()
            .HasFilter("\"ClientPaymentId\" IS NOT NULL")
            .HasDatabaseName("UX_InstallmentPayments_ClientPaymentId");

        builder.HasIndex(x => x.ImportKey)
            .IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_InstallmentPayments_ImportKey");

        builder.HasOne(x => x.Order)
            .WithMany(x => x.InstallmentPayments)
            .HasForeignKey(x => x.OrderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}