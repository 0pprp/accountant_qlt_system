using Domain.Entities.ArchiveAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Archive;

public class ArchiveBundleConfiguration : IEntityTypeConfiguration<ArchiveBundle>
{
    public void Configure(EntityTypeBuilder<ArchiveBundle> builder)
    {
        builder.ToTable("ArchiveBundles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SalesEmployeeName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken().HasDefaultValue(0).IsRequired();

        // الفرع جزء من المفتاح: موظف واحد قد يبيع في فرعين داخل العشرة نفسها.
        builder.HasIndex(x => new
        {
            x.BranchId,
            x.SalesEmployeeUserId,
            x.PeriodYear,
            x.PeriodMonth,
            x.PeriodIndex
        }).IsUnique().HasDatabaseName("UX_ArchiveBundles_Employee_Period");

        builder.HasIndex(x => new { x.BranchId, x.PeriodYear, x.PeriodMonth, x.PeriodIndex });

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesEmployee)
            .WithMany()
            .HasForeignKey(x => x.SalesEmployeeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Bundle)
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ArchiveTransactionConfiguration : IEntityTypeConfiguration<ArchiveTransaction>
{
    public void Configure(EntityTypeBuilder<ArchiveTransaction> builder)
    {
        builder.ToTable("ArchiveTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(30);
        builder.Property(x => x.VoidReason).HasMaxLength(ArchiveTransaction.ReasonMaxLength);
        builder.Property(x => x.LastReturnReason).HasMaxLength(ArchiveTransaction.ReasonMaxLength);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken().HasDefaultValue(0).IsRequired();

        builder.HasIndex(x => x.OrderId).IsUnique().HasDatabaseName("UX_ArchiveTransactions_OrderId");
        builder.HasIndex(x => new { x.BundleId, x.Status });
        builder.HasIndex(x => new { x.FollowerUserId, x.Status });

        // حذف الطلب حذفاً صلباً ما زال ممكناً عبر «أخرِجه من البيع ثم احذفه»،
        // فنمنعه هنا صراحةً بدل أن يمحو سجل العهدة.
        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Follower)
            .WithMany()
            .HasForeignKey(x => x.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Transaction)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArchiveTransactionEventConfiguration : IEntityTypeConfiguration<ArchiveTransactionEvent>
{
    public void Configure(EntityTypeBuilder<ArchiveTransactionEvent> builder)
    {
        builder.ToTable("ArchiveTransactionEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActorName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(ArchiveTransactionEvent.NoteMaxLength);

        builder.HasIndex(x => new { x.TransactionId, x.AtUtc });
        builder.HasIndex(x => x.OperationId);
    }
}

public class ArchiveChangeOutboxConfiguration : IEntityTypeConfiguration<ArchiveChangeOutbox>
{
    public void Configure(EntityTypeBuilder<ArchiveChangeOutbox> builder)
    {
        builder.ToTable("ArchiveChangeOutboxes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChangeType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.TargetStage).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.LastError).HasMaxLength(ArchiveChangeOutbox.LastErrorMaxLength);

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.PublishedAtUtc, x.Id });
    }
}
