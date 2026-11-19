using Application.Archive.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Commands.ReceiveTransactions;

/// <summary>
/// استلام انتقائي: المستلِم يؤشّر الأسماء التي وصلته فعلاً.
/// الدفعة واحدة لا تتجزّأ، والمستلَم سلفاً يُتجاوَز ويُبلَّغ عدده.
/// </summary>
public record ReceiveArchiveTransactionsCommand : IRequest<Result<ArchiveActionResultDto>>
{
    public required List<int> TransactionIds { get; init; }
    public ArchiveStage AtStage { get; init; }
    public string? Note { get; init; }
}

public class ReceiveArchiveTransactionsCommandValidator
    : AbstractValidator<ReceiveArchiveTransactionsCommand>
{
    public ReceiveArchiveTransactionsCommandValidator()
    {
        RuleFor(x => x.TransactionIds).NotEmpty().WithMessage("حدّد معاملة واحدة على الأقل");
        RuleFor(x => x.TransactionIds).Must(x => x.Count <= 500)
            .WithMessage("لا يمكن استلام أكثر من 500 معاملة في دفعة واحدة");
        RuleFor(x => x.AtStage).IsInEnum().WithMessage("مرحلة غير صالحة");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("الملاحظة يجب ألا تتجاوز 500 حرف");
    }
}
