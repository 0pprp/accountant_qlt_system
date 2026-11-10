using Domain.Entities.ComplaintAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Complaint;

public class ComplaintMessageConfiguration : IEntityTypeConfiguration<ComplaintMessage>
{
    public void Configure(EntityTypeBuilder<ComplaintMessage> builder)
    {
        builder.ToTable("ComplaintMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceApp).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SenderDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SenderRole).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BranchName).HasMaxLength(200);
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.BranchId, x.CreatedAtUtc });
    }
}
