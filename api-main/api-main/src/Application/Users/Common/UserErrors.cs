namespace Application.Users.Common;

public static class UserErrors
{
    public static Error UserNotFound = new("لم يتم العثور على المستخدم", "User_Not_Found");
    public static Error UsernameOrPasswordIsNotCorrect = new("اسم المستخدم أو كلمة المرور غير صحيحة", "Username_Or_Password_Is_Not_Correct");
    public static Error AppBlocked = new("تم إيقاف تطبيقك من الإدارة — لا يمكنك الدخول.", "Sales_Employee_App_Blocked");
    public static Error RefreshTokenIsNotValid = new("يرجى تسجيل الدخول مرة أخرى.", "RefreshToken_Is_Not_Valid");
    public static Error UsernameIsAlreadyExist = new("اسم المستخدم مكرر.", "Username_Is_Already_Exist");
    public static Error RoleNotFound = new("لم يتم العثور على الدور", "Role_Not_Found");
    public static Error SomePermissionsNotFound = new("لم يتم العثور على بعض الصلاحيات المحددة", "Some_Permissions_Not_Found");
    public static Error CannotUpdateAdminPermissions = new("لا يمكن تعديل صلاحيات المدير", "Cannot_Update_Admin_Permissions");
    public static Error CannotAssignAdminRole = new("لا يمكن تعيين دور المدير", "Cannot_Assign_Admin_Role");
    public static Error NotAuthorizedToManagePermissions = new("غير مصرح بإدارة تخصيص الصلاحيات", "Not_Authorized_To_Manage_Permissions");
    public static Error CannotManageOwnPermissions = new("لا يمكن تعديل صلاحيات الحساب الحالي", "Cannot_Manage_Own_Permissions");
    public static Error ProtectedAccount = new("هذا الحساب محمي ولا يمكن تعديل صلاحياته", "Protected_Account");
    public static Error InvalidPermissionOverrideMode = new("وضع التخصيص غير صالح", "Invalid_Permission_Override_Mode");
    public static Error DuplicatePermissionOverride = new("تكرار معرف صلاحية في الطلب", "Duplicate_Permission_Override");
    public static Error PermissionsVersionConflict = new("تم تعديل الصلاحيات من مسؤول آخر. أعد التحميل للمقارنة.", "Permissions_Version_Conflict");
    public static Error ExpectedVersionRequired = new("إصدار الصلاحيات مطلوب", "Expected_Version_Required");
    public static Error NotAuthorizedToAssignRoleOrBranches = new(
        "غير مصرح بتعيين الدور أو الفروع. يتطلب سياسة إدارة معتمدة (سوبر أدمن).",
        "Not_Authorized_To_Assign_Role_Or_Branches");
    public static Error BranchesRequired = new("يجب تحديد فرع واحد على الأقل", "Branches_Required");
    public static Error SomeBranchesNotFound = new("لم يتم العثور على بعض الفروع المحددة", "Some_Branches_Not_Found");
    public static Error CannotChangeProtectedAccountRoleOrBranches = new(
        "لا يمكن تعديل دور أو فروع الحساب المحمي",
        "Cannot_Change_Protected_Account_Role_Or_Branches");
}