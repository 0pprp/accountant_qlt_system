namespace Application.Safes.Common;

public static class SafeErrors
{
    public static Error SafeNotFound = new("لم يتم العثور على القاصه", "Safe_Not_Found");
    public static Error SafeAccessDenied = new("ليس لديك صلاحية الوصول إلى هذه القاصه", "Safe_Access_Denied");
    public static Error ThereIsNotEnoughMoneyInTheSafe = new("لا يوجد ما يكفي من المال في القاصه", "There_Is_Not_Enough_Money_In_The_Safe");
    public static Error SellerNotFound = new("لم يتم العثور على البائع", "Seller_Not_Found");
    public static Error RequestedAmountIsGreaterThanSellerUndeliveredCashAmount = new("المبلغ المطلوب أكبر من المبلغ النقدي غير المسلم من البائع", "Requested_Amount_Is_GreaterThan_Seller_UndeliveredCashAmount");
    public static Error SellerDoesNotBelongToThisBranch = new("البائع لا ينتمي إلى هذا الفرع", "Seller_Does_Not_Belong_To_This_Branch");
    public static Error OneOrMoreSellerNotFound = new("لم يتم العثور على بائع واحد أو أكثر", "One_Or_More_Seller_Not_Found");
    public static Error OneOrMoreSellersDoNotBelongToThisBranch = new("لا ينتمي أحد البائعين أو أكثر إلى هذا الفرع", "One_Or_More_Sellers_Do_Not_Belong_To_This_Branch");
    public static Error MoneyTransferBetweenTwoBranchSafeIsNotAllowed = new("لا يُسمح بتحويل الأموال بين فرعين آمنين", "Money_Transfer_Between_Two_Branch_Safe_Is_Not_Allowed");
}