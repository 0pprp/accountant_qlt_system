namespace Application.Committee.Commands.App.FieldFollowups;

public static class CommitteeFieldFollowupErrors
{
    public static Error IncompleteData =
        new("بيانات النقل غير كاملة", "Committee_FieldFollowup_Incomplete");

    public static Error CustomerNotFound =
        new("الزبون غير موجود", "Committee_FieldFollowup_CustomerNotFound");

    public static Error NotFound =
        new("المتابعة غير موجودة", "Committee_FieldFollowup_NotFound");

    public static Error AlreadyCompleted =
        new("هذه المتابعة مكتملة مسبقاً", "Committee_FieldFollowup_AlreadyCompleted");

    public static Error NoteRequired =
        new("الملاحظة إجبارية قبل اعتماد تمت المتابعة", "Committee_FieldFollowup_NoteRequired");
}
