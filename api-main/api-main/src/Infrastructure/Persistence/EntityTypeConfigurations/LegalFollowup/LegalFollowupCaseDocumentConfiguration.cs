using Domain.Entities.LegalFollowupAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.LegalFollowup;

public class LegalFollowupCaseDocumentConfiguration : IEntityTypeConfiguration<LegalFollowupCaseDocument>
{
    public void Configure(EntityTypeBuilder<LegalFollowupCaseDocument> builder)
    {
        builder.ToTable("LegalFollowupCaseDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Extension).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CaseId, x.DocumentType });
        builder.HasOne(x => x.Case)
            .WithMany()
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
