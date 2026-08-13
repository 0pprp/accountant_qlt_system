namespace Application.InstallmentPayments.Common;

public static class InstallmentPaymentErrors
{
    public static Error InstallmentPaymentNotFound = new("لم يتم العثور على التسدید", "InstallmentPayment_Not_Found");
    public static Error CanNotPerformThisAction = new("لا يمكن إجراء هذه العملية", "Can_Not_Perform_This_Action");
}