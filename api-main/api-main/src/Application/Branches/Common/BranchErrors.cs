namespace Application.Branches.Common;

public static class BranchErrors
{
    public static Error BranchAlreadyExist = new("هذا الفرع موجود", "Branch_Already_Exist");
    public static Error BranchNotFound = new("لم يتم العثور على الفرع", "Branch_Not_Found");
    public static Error BranchStillReferencedByWarehouses = new("لا يمكن حذف الفرع: لا تزال تتم الإشارة إليها بواسطة المخزن", "Branch_Still_Referenced_By_Warehouses");
}