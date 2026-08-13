using Application.Common.Extensions;

namespace Application.Safes.Commands.Admin.CreateSafeTransfer;

public class CreateSafeTransferCommandValidator : AbstractValidator<CreateSafeTransferCommand>
{
    public CreateSafeTransferCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون المبلغ أكبر من الصفر")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("يجب ألا يتجاوز ملاحظات 1000 حرف")
            .When(x => x.Description is not null);

        RuleFor(x => x.SourceSafeId)
            .NotEqual(x => x.DestinationSafeId)
            .WithMessage("لا يمكن أن يكون مصدر القاصه ووجهة القاصه متماثلين")
            .When(x => x.SourceSafeId is not null);
    }
}