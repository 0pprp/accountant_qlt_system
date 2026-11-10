namespace Application.SalesFollowup.Common;

public static class SalesFollowupErrors
{
    public static Error BranchForbidden = new("لا صلاحية على هذا الفرع", "SalesFollowup_Branch_Forbidden");
    public static Error BranchRequired = new("معرف الفرع مطلوب", "SalesFollowup_Branch_Required");
    public static Error EmployeeNotFound = new("لم يتم العثور على الموظف", "SalesFollowup_Employee_Not_Found");
    public static Error InvalidManagerScore = new("درجة تقييم المدير غير صالحة", "SalesFollowup_Invalid_Manager_Score");
}
