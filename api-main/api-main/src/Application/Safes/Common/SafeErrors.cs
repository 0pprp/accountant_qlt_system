namespace Application.Safes.Common;

public static class SafeErrors
{
    public static Error SafeNotFound = new("لم يتم العثور على الخزينة", "Safe_Not_Found");
    public static Error SafeAccessDenied = new("ليس لديك صلاحية الوصول إلى هذه الخزينة", "Safe_Access_Denied");
    public static Error SafeIsInactive = new("الخزينة موقوفة ولا تقبل عمليات", "Safe_Is_Inactive");
    public static Error ThereIsNotEnoughMoneyInTheSafe = new("لا يوجد ما يكفي من المال في الخزينة", "There_Is_Not_Enough_Money_In_The_Safe");
    public static Error BranchSafeNotFound = new("لا توجد قاصة لهذا الفرع", "Branch_Safe_Not_Found");
    public static Error BranchIsRequired = new("يجب تحديد الفرع لأن القاصة الرئيسية أُلغيت", "Branch_Is_Required");
    public static Error ExpensesSafeNotFound = new("لا يوجد سجل صرفيات لهذا الفرع", "Expenses_Safe_Not_Found");
    public static Error TransferToSameSafeIsNotAllowed = new("لا يمكن النقل إلى نفس الخزينة", "Transfer_To_Same_Safe_Is_Not_Allowed");
    public static Error TransferBetweenBranchesIsNotAllowed = new("لا يُسمح بالنقل بين خزائن فرعين مختلفين", "Transfer_Between_Branches_Is_Not_Allowed");
    public static Error SystemSafeCannotBeDeleted = new("لا يمكن حذف قاصة الفرع أو سجل الصرفيات", "System_Safe_Cannot_Be_Deleted");
    public static Error SafeWithBalanceCannotBeDeleted = new("لا يمكن حذف خزينة لها رصيد", "Safe_With_Balance_Cannot_Be_Deleted");
    public static Error SafeWithMovementsCannotBeDeleted = new("لا يمكن حذف خزينة لها حركات مسجلة", "Safe_With_Movements_Cannot_Be_Deleted");
    public static Error SafeBindingIsRequired = new("يجب ربط الخزينة بقائمة أو بموظف", "Safe_Binding_Is_Required");
    public static Error SafeBindingMustBeSingle = new("لا يمكن ربط الخزينة بقائمة وموظف في الوقت نفسه", "Safe_Binding_Must_Be_Single");
    public static Error OrderListNotFoundInBranch = new("القائمة غير موجودة في هذا الفرع", "Order_List_Not_Found_In_Branch");
    public static Error UserNotFoundInBranch = new("الموظف غير موجود في هذا الفرع", "User_Not_Found_In_Branch");
    public static Error SafeForThisBindingAlreadyExists = new("توجد خزينة مربوطة بهذه القائمة أو هذا الموظف", "Safe_For_This_Binding_Already_Exists");
    public static Error ListSafeCannotCoverPaymentReversal = new("رصيد قاصة القائمة لا يكفي لعكس التسديد", "List_Safe_Cannot_Cover_Payment_Reversal");
}
