using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Customer;

public class CustomerLocationConfiguration : IEntityTypeConfiguration<CustomerLocation>
{
    public void Configure(EntityTypeBuilder<CustomerLocation> builder)
    {
        builder.ToTable("CustomerLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.HasIndex(x => x.ImportKey).IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_CustomerLocations_ImportKey");
        builder.HasIndex(x => new { x.CustomerId, x.Source });
        builder.HasOne<Domain.Entities.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerShopPhotoConfiguration : IEntityTypeConfiguration<CustomerShopPhoto>
{
    public void Configure(EntityTypeBuilder<CustomerShopPhoto> builder)
    {
        builder.ToTable("CustomerShopPhotos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ContentType)
            .HasMaxLength(CustomerShopPhoto.MaxContentTypeLength).IsRequired();
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.HasIndex(x => x.ImportKey).IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_CustomerShopPhotos_ImportKey");
        builder.HasIndex(x => x.CustomerId);
        builder.HasOne<Domain.Entities.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerDocumentConfiguration : IEntityTypeConfiguration<CustomerDocument>
{
    public void Configure(EntityTypeBuilder<CustomerDocument> builder)
    {
        builder.ToTable("CustomerDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.FileKey).HasMaxLength(CustomerDocument.MaxKeyLength).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(CustomerDocument.MaxNameLength);
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.HasIndex(x => x.ImportKey).IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_CustomerDocuments_ImportKey");
        builder.HasIndex(x => x.CustomerId);
        builder.HasOne<Domain.Entities.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerRatingSnapshotConfiguration : IEntityTypeConfiguration<CustomerRatingSnapshot>
{
    public void Configure(EntityTypeBuilder<CustomerRatingSnapshot> builder)
    {
        builder.ToTable("CustomerRatingSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Origin).HasMaxLength(40).IsRequired();
        builder.Property(x => x.RatingLabel).HasMaxLength(CustomerRatingSnapshot.MaxLabelLength);
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.HasIndex(x => x.ImportKey).IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_CustomerRatingSnapshots_ImportKey");
        builder.HasIndex(x => new { x.CustomerId, x.CaptureDate });
        builder.HasOne<Domain.Entities.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerShopProfileConfiguration : IEntityTypeConfiguration<CustomerShopProfile>
{
    public void Configure(EntityTypeBuilder<CustomerShopProfile> builder)
    {
        builder.ToTable("CustomerShopProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ShopName).HasMaxLength(CustomerShopProfile.MaxNameLength);
        builder.Property(x => x.BusinessType).HasMaxLength(CustomerShopProfile.MaxNameLength);
        builder.Property(x => x.ShopImageKey).HasMaxLength(CustomerDocument.MaxKeyLength);
        builder.Property(x => x.ImportKey).HasMaxLength(120);
        builder.HasIndex(x => x.ImportKey).IsUnique()
            .HasFilter("\"ImportKey\" IS NOT NULL")
            .HasDatabaseName("UX_CustomerShopProfiles_ImportKey");
        builder.HasIndex(x => x.CustomerId);
        builder.HasOne<Domain.Entities.CustomerAggregate.Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
