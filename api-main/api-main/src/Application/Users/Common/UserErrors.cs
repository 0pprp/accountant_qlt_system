namespace Application.Users.Common;

public static class UserErrors
{
    public static Error UserNotFound = new("لم يتم العثور على المستخدم", "User_Not_Found");
    public static Error UsernameOrPasswordIsNotCorrect = new("اسم المستخدم أو كلمة المرور غير صحيحة", "Username_Or_Password_Is_Not_Correct");
    public static Error RefreshTokenIsNotValid = new("يرجى تسجيل الدخول مرة أخرى.", "RefreshToken_Is_Not_Valid");
    public static Error UsernameIsAlreadyExist = new("اسم المستخدم مكرر.", "Username_Is_Already_Exist");
    public static Error RoleNotFound = new("لم يتم العثور على الدور", "Role_Not_Found");
    public static Error SomePermissionsNotFound = new("لم يتم العثور على بعض الصلاحيات المحددة", "Some_Permissions_Not_Found");
    public static Error CannotUpdateAdminPermissions = new("لا يمكن تعديل صلاحيات المدير", "Cannot_Update_Admin_Permissions");
    public static Error CannotAssignAdminRole = new("لا يمكن تعيين دور المدير", "Cannot_Assign_Admin_Role");
}