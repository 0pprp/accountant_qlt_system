using Application.Common.Extensions;
using Domain.Entities.SafeAggregate;
using FluentValidation;

namespace Application.Safes.Commands.Admin.Transfer;

public class TransferBetweenSafesCommandValidator : AbstractValidator<TransferBetweenSafesCommand>
{
    public TransferBetweenSafesCommandValidator()
    {
        RuleFor(x => x.SourceSafeId)
            .GreaterThan(0).WithMessage("يجب تحديد الخزينة المصدر");

        RuleFor(x => x.DestinationSafeId)
            .GreaterThan(0).WithMessage("يجب تحديد الخزينة الهدف")
            .NotEqual(x => x.SourceSafeId).WithMessage("لا يمكن النقل إلى نفس الخزينة");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("يجب أن يكون المبلغ أكبر من الصفر")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Note)
            .MaximumLength(SafeMovement.NoteMaxLength)
            .WithMessage("الملاحظة أطول من المسموح");
    }
}
