using Application.Archive.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Commands.SendBundle;

/// <summary>
/// إرسال كل ما في الحزمة وهو الآن بعهدة المرحلة المُرسِلة.
/// لا قائمة معرّفات من العميل: فمعاملة دخلت بعد آخر تحديث للشاشة لا تُنسى ولا تُرسَل مرتين.
/// </summary>
public record SendArchiveBundleCommand : IRequest<Result<ArchiveActionResultDto>>
{
    public int BundleId { get; init; }
    public ArchiveStage FromStage { get; init; }
    public string? Note { get; init; }
}

public class SendArchiveBundleCommandValidator : AbstractValidator<SendArchiveBundleCommand>
{
    public SendArchiveBundleCommandValidator()
    {
        RuleFor(x => x.BundleId).GreaterThan(0).WithMessage("الحزمة مطلوبة");
        RuleFor(x => x.FromStage).IsInEnum().WithMessage("مرحلة غير صالحة");
        RuleFor(x => x.Note).MaximumLength(500).WithMessage("الملاحظة يجب ألا تتجاوز 500 حرف");
    }
}
