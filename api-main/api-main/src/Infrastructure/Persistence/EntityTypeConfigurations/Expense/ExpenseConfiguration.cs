using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Expense;

public class ExpenseConfiguration : IEntityTypeConfiguration<Domain.Entities.ExpenseAggregate.Expense>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ExpenseAggregate.Expense> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FactorNumber).IsRequired();
        builder.Property(x => x.TotalAmount).IsRequired();
        builder.Property(x => x.SafeType).IsRequired();

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ExpenseItems)
            .WithOne(x => x.Expense)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Expense)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}