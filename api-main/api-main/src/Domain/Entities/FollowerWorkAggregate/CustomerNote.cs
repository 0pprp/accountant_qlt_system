using Domain.Common;

namespace Domain.Entities.FollowerWorkAggregate;

/// <summary>
/// ملاحظة زبون مشتركة (CustomerNotes) — يكتبها المتابع ميدانيًا؛ اللوحة تعرض فقط.
/// ليست ملاحظة مندوب ولا ملاحظة مصادقة بيع.
/// </summary>
public class CustomerNote : Entity
{
    public const int MaxTextLength = 2000;

    private CustomerNote()
    {
    }

    public CustomerNote(
        int customerId,
        int? orderListId,
        int? branchId,
        string noteText,
        int? authorUserId,
        string? authorName,
        DateTimeOffset createdAtUtc)
    {
        CustomerId = customerId;
        OrderListId = orderListId;
        BranchId = branchId;
        NoteText = noteText;
        AuthorUserId = authorUserId;
        AuthorName = authorName;
        CreatedAtUtc = createdAtUtc;
    }

    public int CustomerId { get; private set; }
    /// <summary>قائمة التكليف وقت الكتابة — لنطاق عمل المتابعين.</summary>
    public int? OrderListId { get; private set; }
    public int? BranchId { get; private set; }
    public string NoteText { get; private set; } = "";
    public int? AuthorUserId { get; private set; }
    public string? AuthorName { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
