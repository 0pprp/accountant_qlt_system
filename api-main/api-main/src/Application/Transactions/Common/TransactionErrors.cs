namespace Application.Transactions.Common;

public static class TransactionErrors
{
    public static Error TransactionNotFound = new("لم يتم العثور على عملية", "Transaction_Not_Found");
    public static Error SafeHasNotEnoughMoneyToCompleteThisTransaction = new("لا يوجد في القاصه ما يكفي من المال لإتمام هذه المعاملة", "Safe_Has_Not_Enough_Money_To_Complete_This_Transaction");
}