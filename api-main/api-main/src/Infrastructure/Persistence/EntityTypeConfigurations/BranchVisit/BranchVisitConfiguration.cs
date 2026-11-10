using Domain.Entities.BranchVisitAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.BranchVisit;

public class BranchVisitConfiguration : IEntityTypeConfiguration<Domain.Entities.BranchVisitAggregate.BranchVisit>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.BranchVisitAggregate.BranchVisit> builder)
    {
        builder.ToTable("BranchVisits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpenedByName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DepartNote).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.OpenedByUserId, x.Status });
        builder.HasIndex(x => new { x.BranchId, x.ArrivedAtUtc });
        builder.HasMany(x => x.Ratings)
            .WithOne()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Ratings).HasField("_ratings");
        builder.Navigation(x => x.Members).HasField("_members");
        builder.Navigation(x => x.Attachments).HasField("_attachments");
    }
}

public class BranchVisitRatingConfiguration : IEntityTypeConfiguration<BranchVisitRating>
{
    public void Configure(EntityTypeBuilder<BranchVisitRating> builder)
    {
        builder.ToTable("BranchVisitRatings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ListName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.VisitId, x.OrderListId }).IsUnique();
    }
}

public class BranchVisitMemberConfiguration : IEntityTypeConfiguration<BranchVisitMember>
{
    public void Configure(EntityTypeBuilder<BranchVisitMember> builder)
    {
        builder.ToTable("BranchVisitMembers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.VisitId, x.UserId }).IsUnique();
    }
}

public class BranchVisitAttachmentConfiguration : IEntityTypeConfiguration<BranchVisitAttachment>
{
    public void Configure(EntityTypeBuilder<BranchVisitAttachment> builder)
    {
        builder.ToTable("BranchVisitAttachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Extension).HasMaxLength(16).IsRequired();
        builder.Property(x => x.ContentSha256).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.VisitId, x.ContentSha256 }).IsUnique();
    }
}
