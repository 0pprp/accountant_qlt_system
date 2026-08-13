namespace Application.Orders.Common;

public static class OrderErrors
{
    public static Error OrderNotFound = new("لم يتم العثور على مبيع.", "Order_Not_Found");
    public static Error TheSellerDoesNotBelongToThisOrderList = new("البائع لا ينتمي إلى قائمة الطلبات هذه", "The_Seller_Does_Not_Belong_To_This_OrderList");
    public static Error OrderListDoesNotBelongToOrderBranch = new("القائمة لا تنتمي إلى فرع البيع", "OrderList_Does_Not_Belong_To_Order_Branch");
    public static Error SomeInstallmentsOfThisOrderHaveBeenCollected = new("لقد تم استلام بعض تسدیدات هذا الطلب", "Some_Installments_Of_This_Order_Have_Been_Collected");
    public static Error SomeSelectedProductsNotFound = new("لم يتم العثور على بعض المنتجات المحددة", "Some_Selected_Products_Not_Found");
    public static Error ThereIsNotEnoughQuantityOfThisProduct = new("لا توجد كمية كافية من هذا المنتج", "There_Is_Not_Enough_Quantity_Of_This_Product");
    public static Error YouCanNotAddOrderForMoreThanThreeDaysAgo = new("لا يمكنك إضافة طلبات مضى عليها أكثر من ثلاثة أيام", "You_Can_Not_Add_Order_For_More_Than_Three_Days_Ago");
    public static Error YouCanNotUpdateOrderToMoreThanTwoDaysAgo = new("لا يمكنك تحديث الطلب لأكثر من يومين مضت", "You_Can_Not_Update_Order_To_More_Than_Two_Days_Ago");
    public static Error PaidAmountCanNotBeGreaterThanSellAmount = new("لا يجوز أن يتجاوز المبلغ المدفوع مبلغ البيع", "Paid_Amount_Can_Not_Be_Greater_Than_Sell_Amount");
    public static Error OrderInfoHasBeenCompletedBefore = new("تم تنفيذ معلومات الطلب مسبقًا", "Order_Info_Has_Been_Completed_Before");
    public static Error CustomerDoesNotBelongToYourBranch = new("العميل لا ينتمي إلى فرعكم", "Customer_Does_Not_Belong_To_Your_Branch");
    public static Error AttachmentsStepAlreadyCompleted = new("تم إكمال مرحلة مرفقات الطلب مسبقًا", "Attachments_Step_Already_Completed");
    public static Error RequiredOrderAttachmentsAreMissing = new("يجب رفع مرفقات وصل الشراء و وصل أمانة و عقد البيع", "Required_Order_Attachments_Are_Missing");
    public static Error OrderDoesNotBelongToYou = new("الطلب ليس ملكك", "Order_Does_Not_Belong_To_You");
    public static Error CanNotSetSellerInfoInCurrentStep = new("لا يمكن تحديد معلومات البائع في هذه المرحلة.", "Can_Not_Set_SellerInfo_In_Current_Step");
    public static Error YouDoNotHaveAnyOrderList = new("ليس لديك أي قائمة طلبات", "You_Have_Not_Any_OrderList");
    public static Error CreationAddressIsRequired = new("عنوان البيع مطلوب", "CreationAddress_Is_Required");
    public static Error SaleDateIsRequired = new("تسجيل تاريخ الطلب إلزامي", "SaleDate_Is_Required");
    public static Error SaleTimeIsRequired = new("وقت البيع مطلوب", "SaleTime_Is_Required");
    public static Error OrderListIdIsRequired = new("قائمة الطلبات مطلوبة", "OrderListId_Is_Required");
    public static Error MandobCanNotSetOrderListId = new("لا يمكن للمندوب تحديد قائمة الطلبات", "Mandob_Can_Not_Set_OrderListId");
    public static Error YouDoNotHavePermissionForThisAction = new("ليس لديك إذن للقيام بهذا الإجراء", "You_Do_Not_Have_Permission_For_This_Action");
    public static Error YouCanNotDeleteThisOrder = new("لا يمكنك حذف هذا مبيع.", "You_Can_Not_Delete_This_Order");
    public static Error OrderInformationHasNotCompletedYet = new("معلومات الطلب غير مكتملة بعد.", "Order_Information_Has_Not_Completed_Yet");
    public static Error ApprovalStatusOfOrderHasBeenSetBefore = new("تم تحديد حالة الموافقة على الطلب مسبقًا", "ApprovalStatus_Of_Order_Has_Been_Set_Before");
}