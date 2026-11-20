using Domain.Entities.FieldPromoterAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FieldPromoter;

public class FieldPromoterProfileConfiguration : IEntityTypeConfiguration<FieldPromoterProfile>
{
    public void Configure(EntityTypeBuilder<FieldPromoterProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SuspendReason).HasMaxLength(FieldPromoterProfile.MaxReasonLength);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FieldPromoterPinConfiguration : IEntityTypeConfiguration<FieldPromoterPin>
{
    public void Configure(EntityTypeBuilder<FieldPromoterPin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(FieldPromoterPin.MaxNameLength).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Note).HasMaxLength(FieldPromoterPin.MaxNoteLength);
        builder.HasIndex(x => new { x.Latitude, x.Longitude }).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FieldPromoterOrderShopConfiguration : IEntityTypeConfiguration<FieldPromoterOrderShop>
{
    public void Configure(EntityTypeBuilder<FieldPromoterOrderShop> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PhotoContentType).HasMaxLength(40);
        builder.HasIndex(x => x.SalesRequestId).IsUnique();
        builder.HasOne<Domain.Entities.SalesRequestAggregate.SalesRequest>().WithMany().HasForeignKey(x => x.SalesRequestId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FieldPromoterAlertConfiguration : IEntityTypeConfiguration<FieldPromoterAlert>
{
    public void Configure(EntityTypeBuilder<FieldPromoterAlert> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.ResolvedAtUtc });
        builder.HasIndex(x => new { x.RaisedAtUtc, x.SeenAtUtc });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FieldPromoterDayConfiguration : IEntityTypeConfiguration<FieldPromoterDay>
{
    public void Configure(EntityTypeBuilder<FieldPromoterDay> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.Day }).IsUnique();
        builder.HasIndex(x => x.Day);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FieldPromoterSettingsConfiguration : IEntityTypeConfiguration<FieldPromoterSettings>
{
    public void Configure(EntityTypeBuilder<FieldPromoterSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SupportPhone).HasMaxLength(FieldPromoterSettings.MaxSupportLength);
        builder.Property(x => x.SupportWhatsapp).HasMaxLength(FieldPromoterSettings.MaxSupportLength);
    }
}
