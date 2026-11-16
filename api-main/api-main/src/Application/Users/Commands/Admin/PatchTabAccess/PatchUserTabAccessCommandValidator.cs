using FluentValidation;

namespace Application.Users.Commands.Admin.PatchTabAccess;

public class PatchUserTabAccessCommandValidator : AbstractValidator<PatchUserTabAccessCommand>
{
    public PatchUserTabAccessCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Changes).NotNull();
    }
}
