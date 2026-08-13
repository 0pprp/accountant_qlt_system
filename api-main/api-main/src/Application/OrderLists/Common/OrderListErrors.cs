namespace Application.OrderLists.Common;

public static class OrderListErrors
{
    public static Error OrderListNotFound = new("لم يتم العثور على القائمة", "OrderList_Not_Found");
    public static Error OrderListAlreadyExist = new("هذا القائمة موجود", "OrderList_Already_Exist");
    public static Error MandobNotFound = new("لم يتم العثور على المندوب", "Mandob_Not_Found");
    public static Error MotabaNotFound = new("لم يتم العثور على المتابع", "Motaba_Not_Found");
    public static Error BranchNotFound = new("لم يتم العثور على الفرع", "Branch_Not_Found");
    public static Error YouHaveNotOrderListYet = new("لا توجد لديكم القائمة في الوقت الحالي", "You_Have_Not_OrderList_Yet");
    public static Error MandobAlreadyHasAnOrderList = new("المندوب لديه قائمة بالفعل.", "Mandob_Already_Has_An_OrderList");
}