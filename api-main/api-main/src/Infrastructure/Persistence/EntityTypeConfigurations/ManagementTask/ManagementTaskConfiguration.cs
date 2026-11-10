using Domain.Entities.ManagementTaskAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskEntity = Domain.Entities.ManagementTaskAggregate.ManagementTask;

namespace Infrastructure.Persistence.EntityTypeConfigurations.ManagementTask;

public class ManagementTaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("ManagementTasks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.AssigneeDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CurrentHolderDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RejectNote).HasMaxLength(1000);
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.DueDate).IsRequired();
        builder.Property(x => x.ConcurrencyStamp)
            .IsConcurrencyToken()
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => new { x.CurrentHolderUserId, x.Status });
        builder.HasIndex(x => x.AssigneeUserId);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.OrderId);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssigneeUser)
            .WithMany()
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CurrentHolderUser)
            .WithMany()
            .HasForeignKey(x => x.CurrentHolderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LinkedOrder)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ManagementTaskEventConfiguration : IEntityTypeConfiguration<ManagementTaskEvent>
{
    public void Configure(EntityTypeBuilder<ManagementTaskEvent> builder)
    {
        builder.ToTable("ManagementTaskEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.AtUtc).IsRequired();
        builder.Property(x => x.ActorDisplayName).HasMaxLength(200);
        builder.Property(x => x.TargetDisplayName).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasIndex(x => new { x.TaskId, x.AtUtc });
    }
}

public class ManagementTaskAttachmentConfiguration : IEntityTypeConfiguration<ManagementTaskAttachment>
{
    public void Configure(EntityTypeBuilder<ManagementTaskAttachment> builder)
    {
        builder.ToTable("ManagementTaskAttachments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();
        builder.Property(x => x.UploadedAtUtc).IsRequired();

        builder.HasIndex(x => x.TaskId);
    }
}

public class ManagementTaskChangeOutboxConfiguration : IEntityTypeConfiguration<ManagementTaskChangeOutbox>
{
    public void Configure(EntityTypeBuilder<ManagementTaskChangeOutbox> builder)
    {
        builder.ToTable("ManagementTaskChangeOutboxes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.ChangeType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(ManagementTaskChangeOutbox.LastErrorMaxLength);
        builder.Property(x => x.RetryCount).HasDefaultValue(0);

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.PublishedAtUtc, x.Id });
        builder.HasIndex(x => new { x.TaskId, x.Revision });
    }
}
