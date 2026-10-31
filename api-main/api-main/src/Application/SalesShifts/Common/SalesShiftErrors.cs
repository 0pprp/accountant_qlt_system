namespace Application.SalesShifts.Common;

public static class SalesShiftErrors
{
    public static Error ActiveShiftAlreadyExists = new("يوجد دوام نشط بالفعل.", "Active_Shift_Already_Exists");
    public static Error ActiveShiftNotFound = new("لا يوجد دوام نشط.", "Active_Shift_Not_Found");
    public static Error BranchNotFound = new("لم يتم العثور على فرع للمستخدم.", "User_Branch_Not_Found");
}
