using Domain.Common;

namespace Domain.Entities.FollowerWorkAggregate;

/// <summary>
/// ملاحظة يكتبها المتابع عن مندوب/قائمة (FollowerDelegateNotes).
/// ليست CustomerNote وليست ملاحظة كتبها المندوب.
/// غير قابلة للتعديل/الحذف بعد الحفظ.
/// </summary>
public class FollowerDelegateNote : Entity
{
    public const int MaxTextLength = 2000;
    public const string RoleFollower = "Follower";

    private FollowerDelegateNote()
    {
    }

    public FollowerDelegateNote(
        int orderListId,
        int mandobUserId,
        string noteText,
        int createdByUserId,
        string? createdByName,
        DateTimeOffset createdAtUtc)
    {
        OrderListId = orderListId;
        MandobUserId = mandobUserId;
        NoteText = noteText;
        CreatedByUserId = createdByUserId;
        CreatedByName = createdByName;
        CreatedByRole = RoleFollower;
        CreatedAtUtc = createdAtUtc;
    }

    public int OrderListId { get; private set; }
    /// <summary>مندوب القائمة وقت الكتابة (للهوية التاريخية).</summary>
    public int MandobUserId { get; private set; }
    public string NoteText { get; private set; } = "";
    public int CreatedByUserId { get; private set; }
    public string? CreatedByName { get; private set; }
    public string CreatedByRole { get; private set; } = RoleFollower;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
