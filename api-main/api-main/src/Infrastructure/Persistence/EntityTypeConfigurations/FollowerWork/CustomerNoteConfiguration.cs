using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FollowerWork;

public class CustomerNoteConfiguration : IEntityTypeConfiguration<CustomerNote>
{
    public void Configure(EntityTypeBuilder<CustomerNote> builder)
    {
        builder.ToTable("CustomerNotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NoteText).HasMaxLength(CustomerNote.MaxTextLength).IsRequired();
        builder.Property(x => x.AuthorName).HasMaxLength(150);
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.OrderListId, x.CreatedAtUtc });
    }
}
