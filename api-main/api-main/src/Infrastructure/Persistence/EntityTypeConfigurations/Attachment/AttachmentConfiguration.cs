using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Attachment;

public class AttachmentConfiguration : IEntityTypeConfiguration<Domain.Entities.AttachmentAggregate.Attachment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.AttachmentAggregate.Attachment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
        builder.Property(x => x.FileSizeInByte).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.FileType).IsRequired();
    }
}