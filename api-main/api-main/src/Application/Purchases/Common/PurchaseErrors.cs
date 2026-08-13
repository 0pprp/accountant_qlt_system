namespace Application.Purchases.Common;

public static class PurchaseErrors
{
    public static Error PurchaseWithThisFactorNumberAlreadyExist = new("الشراء بهذا الفاتوره موجود بالفعل", "Purchase_With_This_FactorNumber_Already_Exist");
    public static Error SomeProductsNotFound = new("لم يتم العثور على بعض المنتجات", "Some_Products_Not_Found");
    public static Error PurchaseNotFound = new("لم يتم العثور على الشراء", "Purchase_Not_Found");
    public static Error YouAreNotAllowedToDoThisAction = new("لا يُسمح لك بالقيام بهذا الإجراء", "You_Are_Not_Allowed_To_Do_This_Action");
}