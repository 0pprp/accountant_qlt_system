using Application.Archive.Common;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Commands.ReturnTransactions;

/// <summary>الاسترجاع لنقص — مسؤول الأرشيف وحده، ويختار الوجهة بسبب إجباري.</summary>
public record ReturnArchiveTransactionsCommand : IRequest<Result<ArchiveActionResultDto>>
{
    public required List<int> TransactionIds { get; init; }
    public ArchiveStage Destination { get; init; }
    public required string Reason { get; init; }
}

public class ReturnArchiveTransactionsCommandValidator
    : AbstractValidator<ReturnArchiveTransactionsCommand>
{
    public ReturnArchiveTransactionsCommandValidator()
    {
        RuleFor(x => x.TransactionIds).NotEmpty().WithMessage("حدّد معاملة واحدة على الأقل");
        RuleFor(x => x.TransactionIds).Must(x => x.Count <= 500)
            .WithMessage("لا يمكن استرجاع أكثر من 500 معاملة في دفعة واحدة");
        RuleFor(x => x.Destination).IsInEnum().WithMessage("وجهة غير صالحة");
        RuleFor(x => x.Destination).NotEqual(ArchiveStage.Archive)
            .WithMessage("وجهة الاسترجاع يجب أن تكون مرحلة سابقة");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب الاسترجاع إجباري")
            .MinimumLength(ArchiveTransaction.ReasonMinLength)
            .WithMessage("اكتب سبباً مفهوماً للاسترجاع")
            .MaximumLength(ArchiveTransaction.ReasonMaxLength)
            .WithMessage("السبب يجب ألا يتجاوز 500 حرف");
    }
}
