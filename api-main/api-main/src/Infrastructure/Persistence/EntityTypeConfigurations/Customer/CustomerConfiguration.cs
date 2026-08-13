using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Customer;

public class CustomerConfiguration : IEntityTypeConfiguration<Domain.Entities.CustomerAggregate.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.CustomerAggregate.Customer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MotherName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NationalCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BirthDate).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.WhatsAppPhoneNumber).HasMaxLength(30).IsRequired();
        builder.ComplexProperty(x => x.Business, propertyBuilder =>
        {
            propertyBuilder.Property(b => b.Name).HasMaxLength(100).IsRequired();
            propertyBuilder.Property(b => b.Address).HasMaxLength(300).IsRequired();
            propertyBuilder.Property(b => b.NearestKnownLocation).HasMaxLength(200).IsRequired();
        });

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}