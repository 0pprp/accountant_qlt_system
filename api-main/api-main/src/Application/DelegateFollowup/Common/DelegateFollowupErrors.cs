namespace Application.DelegateFollowup.Common;

public static class DelegateFollowupErrors
{
    public static Error BranchForbidden = new("لا صلاحية على هذا الفرع", "DelegateFollowup_Branch_Forbidden");
    public static Error BranchRequired = new("معرف الفرع مطلوب", "DelegateFollowup_Branch_Required");
    public static Error ListNotFound = new("لم يتم العثور على القائمة", "DelegateFollowup_List_Not_Found");
    public static Error InvalidBoardScore = new("التقييم يجب أن يكون بين 0 و 10", "DelegateFollowup_Invalid_Board_Score");
    public static Error ActorRequired = new("المستخدم الحالي غير معروف", "DelegateFollowup_Actor_Forbidden");
}
