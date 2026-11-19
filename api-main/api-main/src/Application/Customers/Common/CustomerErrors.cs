namespace Application.Customers.Common;

public static class CustomerErrors
{
    public static Error CustomerNotFound = new("لم يتم العثور على العمیل", "Customer_Not_Found");
    public static Error CustomerWithThisNationalCodeAlreadyExist = new("العمیل بهذا رقم الهوية متاح في التطبيق.", "Customer_With_This_NationalCode_Already_Exist");
    public static Error CustomerDiscountExceedsOrderSellAmount = new("خصم سعر البيع يساوي أو يتجاوز مبلغ البيع في أحد مبيعات العميل", "Customer_Discount_Exceeds_Order_SellAmount");
    public static Error CustomerDiscountExceedsOrderRemainingAmount = new("خصم سعر البيع أكبر من المبلغ المتبقي في أحد مبيعات العميل", "Customer_Discount_Exceeds_Order_Remaining_Amount");
    public static Error CustomerDiscountExceedsOrderDailyInstallment = new("خصم القسط اليومي يساوي أو يتجاوز مبلغ القسط اليومي في أحد مبيعات العميل", "Customer_Discount_Exceeds_Order_Daily_Installment");
    public static Error CustomerDiscountCanNotRaiseCompletedOrderAmount = new("لا يمكن تقليل الخصم لأن أحد مبيعات العميل مكتمل السداد", "Customer_Discount_Can_Not_Raise_Completed_Order_Amount");
}