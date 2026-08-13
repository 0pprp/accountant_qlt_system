namespace Application.Customers.Common;

public static class CustomerErrors
{
    public static Error CustomerNotFound = new("لم يتم العثور على العمیل", "Customer_Not_Found");
    public static Error CustomerWithThisNationalCodeAlreadyExist = new("العمیل بهذا رقم الهوية متاح في التطبيق.", "Customer_With_This_NationalCode_Already_Exist");
}