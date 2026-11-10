using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.FollowerWork;

public class FollowerDelegateNoteConfiguration : IEntityTypeConfiguration<FollowerDelegateNote>
{
    public void Configure(EntityTypeBuilder<FollowerDelegateNote> builder)
    {
        builder.ToTable("FollowerDelegateNotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NoteText).HasMaxLength(FollowerDelegateNote.MaxTextLength).IsRequired();
        builder.Property(x => x.CreatedByName).HasMaxLength(150);
        builder.Property(x => x.CreatedByRole).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.OrderListId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.CreatedByUserId, x.CreatedAtUtc });
    }
}
