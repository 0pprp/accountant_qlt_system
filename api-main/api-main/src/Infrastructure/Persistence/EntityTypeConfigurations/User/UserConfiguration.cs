using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.User;

public class UserConfiguration : IEntityTypeConfiguration<Domain.Entities.UserAggregate.User>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.UserAggregate.User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MotherName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired(false);
        builder.Property(x => x.NationalCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BirthDate).IsRequired();
        builder.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreationStep).IsRequired();
        builder.ComplexProperty(x => x.SalaryDetail, propertyBuilder =>
        {
            propertyBuilder.Property(x => x.Type).IsRequired(false);
            propertyBuilder.Property(x => x.Amount).IsRequired(false);
            propertyBuilder.Property(x => x.SaleSharePercent).IsRequired(false);
            propertyBuilder.Property(x => x.InstallmentSharePercent).IsRequired(false);
        });
        builder.Property(x => x.Address).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();

        builder.HasOne(x => x.OrderListAsMandob)
            .WithOne(x => x.Mandob)
            .HasForeignKey<Domain.Entities.OrderListAggregate.OrderList>(x => x.MandobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OrderListsAsMotaba)
            .WithOne(x => x.Motaba)
            .HasForeignKey(x => x.MotabaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.UserLogins)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserPermissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserBranches)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}