using Application.Common.Extensions;
using Domain.Entities.SafeAggregate;
using FluentValidation;

namespace Application.Safes.Commands.Admin.Withdraw;

public class WithdrawFromSafeCommandValidator : AbstractValidator<WithdrawFromSafeCommand>
{
    public WithdrawFromSafeCommandValidator()
    {
        RuleFor(x => x.SafeId)
            .GreaterThan(0).WithMessage("يجب تحديد الخزينة");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("يجب أن يكون المبلغ أكبر من الصفر")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("الملاحظة إلزامية عند السحب")
            .MaximumLength(SafeMovement.NoteMaxLength)
            .WithMessage("الملاحظة أطول من المسموح");
    }
}
