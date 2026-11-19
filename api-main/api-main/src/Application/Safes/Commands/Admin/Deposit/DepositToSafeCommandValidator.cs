using Application.Common.Extensions;
using Domain.Entities.SafeAggregate;
using FluentValidation;

namespace Application.Safes.Commands.Admin.Deposit;

public class DepositToSafeCommandValidator : AbstractValidator<DepositToSafeCommand>
{
    public DepositToSafeCommandValidator()
    {
        RuleFor(x => x.SafeId)
            .GreaterThan(0).WithMessage("يجب تحديد الخزينة");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("يجب أن يكون المبلغ أكبر من الصفر")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Note)
            .MaximumLength(SafeMovement.NoteMaxLength)
            .WithMessage("الملاحظة أطول من المسموح");
    }
}
