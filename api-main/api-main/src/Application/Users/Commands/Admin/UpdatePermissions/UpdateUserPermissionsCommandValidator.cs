using Application.Users.Common;

namespace Application.Users.Commands.Admin.UpdatePermissions;

public class UpdateUserPermissionsCommandValidator : AbstractValidator<UpdateUserPermissionsCommand>
{
    public UpdateUserPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PermissionIds).NotNull();
        RuleFor(x => x.ExpectedVersion)
            .GreaterThanOrEqualTo(0)
            .WithMessage(UserErrors.ExpectedVersionRequired.Message);
    }
}
