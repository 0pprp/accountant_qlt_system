namespace Domain.Entities.ArchiveAggregate.Enums;

/// <summary>
/// موضع ملفّ المعاملة الآن. حقل واحد يجيب دائماً «أين هو».
/// الحالات الفردية عهدة لدى طرف، والزوجية انتقال بلا حامل حتى يُستلم.
/// الإبطال ليس حالة هنا: <c>ArchiveTransaction.IsVoided</c> علم مستقل لأن الورق
/// يبقى فيزيائياً عند حامله حتى لو بطل المبيع.
/// </summary>
public enum ArchiveTransactionStatus : byte
{
    WithSalesEmployee = 0,
    SentToBranch = 1,
    AtBranch = 2,
    SentToFollower = 3,
    AtFollower = 4,
    SentToArchive = 5,

    /// <summary>آخر محطة، وليست نهائية — الاسترجاع يُخرجها منها.</summary>
    Archived = 6
}
