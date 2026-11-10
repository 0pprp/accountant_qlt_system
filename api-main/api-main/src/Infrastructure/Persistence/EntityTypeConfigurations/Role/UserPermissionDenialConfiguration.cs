using Domain.Entities.RoleAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityTypeConfigurations.Role;

public class UserPermissionDenialConfiguration : IEntityTypeConfiguration<UserPermissionDenial>
{
    public void Configure(EntityTypeBuilder<UserPermissionDenial> builder)
    {
        builder.ToTable("UserPermissionDenials");
        builder.HasKey(x => new { x.UserId, x.PermissionId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPermissionDenials)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.UserPermissionDenials)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
