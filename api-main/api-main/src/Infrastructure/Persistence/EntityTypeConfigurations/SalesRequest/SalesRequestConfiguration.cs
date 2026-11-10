using Domain.Entities.SalesRequestAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.SalesRequest;

public class SalesRequestConfiguration : IEntityTypeConfiguration<Domain.Entities.SalesRequestAggregate.SalesRequest>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SalesRequestAggregate.SalesRequest> builder)
    {
        builder.ToTable("SalesRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.RequestedProduct).HasMaxLength(200);
        builder.Property(x => x.SubmissionNote).HasMaxLength(1000);
        builder.Property(x => x.SubmissionFingerprint).HasMaxLength(64);
        builder.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique();

        builder.Property(x => x.CustomerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(30);
        builder.Property(x => x.OriginalExternalId).HasMaxLength(100);
        builder.Property(x => x.RejectReason).HasMaxLength(500);
        builder.Property(x => x.LastStatusNote).HasMaxLength(500);
        builder.Property(x => x.DeleteReason).HasMaxLength(500);
        builder.Property(x => x.SourceType).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.ConcurrencyStamp)
            .IsConcurrencyToken()
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.Status, x.TargetEmployeeId });
        builder.HasIndex(x => x.CustomerPhone);
        builder.HasIndex(x => x.CustomerName);
        builder.HasIndex(x => x.OriginalExternalId);
        builder.HasIndex(x => x.TargetEmployeeId);
        builder.HasIndex(x => new { x.BranchId, x.DeletedAtUtc })
            .HasFilter("\"IsDeleted\" = true");
        builder.HasIndex(x => new { x.BranchId, x.ExceptionHoldStatus })
            .HasFilter("\"ExceptionHoldStatus\" IS NOT NULL");
        builder.HasIndex(x => x.LinkedOrderId)
            .IsUnique()
            .HasFilter("\"LinkedOrderId\" IS NOT NULL");

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ExistingCustomer)
            .WithMany()
            .HasForeignKey(x => x.ExistingCustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TargetEmployee)
            .WithMany()
            .HasForeignKey(x => x.TargetEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LinkedOrder)
            .WithMany()
            .HasForeignKey(x => x.LinkedOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesRequestChangeOutboxConfiguration : IEntityTypeConfiguration<SalesRequestChangeOutbox>
{
    public void Configure(EntityTypeBuilder<SalesRequestChangeOutbox> builder)
    {
        builder.ToTable("SalesRequestChangeOutbox");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.ChangeType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(SalesRequestChangeOutbox.LastErrorMaxLength);
        builder.Property(x => x.RetryCount).HasDefaultValue(0);

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.PublishedAtUtc, x.Id });
        builder.HasIndex(x => new { x.RequestId, x.Revision });
    }
}

public class SalesRequestTimelineEntryConfiguration : IEntityTypeConfiguration<SalesRequestTimelineEntry>
{
    public void Configure(EntityTypeBuilder<SalesRequestTimelineEntry> builder)
    {
        builder.ToTable("SalesRequestTimelineEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.OperationId).IsRequired();
        builder.HasIndex(x => new { x.SalesRequestId, x.OccurredAtUtc });
        builder.HasIndex(x => x.OperationId);

        builder.HasOne(x => x.SalesRequest)
            .WithMany()
            .HasForeignKey(x => x.SalesRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
