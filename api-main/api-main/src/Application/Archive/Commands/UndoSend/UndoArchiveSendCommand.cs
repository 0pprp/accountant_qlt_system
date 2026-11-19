using Application.Archive.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Commands.UndoSend;

/// <summary>
/// تصحيح المُرسِل لفعل نفسه — «أرسلتُ بالخطأ» أو «الظرف وصل بلا الملف».
/// ليس استرجاعاً: الاسترجاع حكمٌ على النقص ولمسؤول الأرشيف وحده.
/// </summary>
public record UndoArchiveSendCommand : IRequest<Result<ArchiveActionResultDto>>
{
    public required List<int> TransactionIds { get; init; }
    public ArchiveStage FromStage { get; init; }
    public string? Note { get; init; }
}

public class UndoArchiveSendCommandValidator : AbstractValidator<UndoArchiveSendCommand>
{
    public UndoArchiveSendCommandValidator()
    {
        RuleFor(x => x.TransactionIds).NotEmpty().WithMessage("حدّد معاملة واحدة على الأقل");
        RuleFor(x => x.FromStage).IsInEnum().WithMessage("مرحلة غير صالحة");
        RuleFor(x => x.FromStage).NotEqual(ArchiveStage.Archive)
            .WithMessage("الأرشيف لا يرسل فلا إلغاء له");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("الملاحظة يجب ألا تتجاوز 500 حرف");
    }
}
