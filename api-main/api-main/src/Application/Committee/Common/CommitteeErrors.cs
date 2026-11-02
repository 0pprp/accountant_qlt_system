namespace Application.Committee.Common;

public static class CommitteeErrors
{
    public static Error NotCommitteeOfficer =
        new("الحساب غير مخوّل لتطبيق مسؤول اللجنة", "Committee_Not_Officer");

    public static Error ListNotFound =
        new("القائمة غير موجودة أو غير مخصصة لك", "Committee_List_Not_Found");

    public static Error NoLists =
        new("لا توجد قوائم مخصصة لك حاليًا", "Committee_No_Lists");
}
