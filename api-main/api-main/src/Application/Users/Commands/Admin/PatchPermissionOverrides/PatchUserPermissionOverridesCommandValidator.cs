using Application.Users.Common;

namespace Application.Users.Commands.Admin.PatchPermissionOverrides;

public class PatchUserPermissionOverridesCommandValidator
    : AbstractValidator<PatchUserPermissionOverridesCommand>
{
    public PatchUserPermissionOverridesCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Changes).NotNull();
        RuleForEach(x => x.Changes).ChildRules(change =>
        {
            change.RuleFor(c => c.PermissionId).GreaterThan(0);
            change.RuleFor(c => c.Mode).IsInEnum();
        });
    }
}
