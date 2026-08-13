namespace Application.Expenses.Common;

public static class ExpenseErrors
{
    public static Error ExpenseWithThisFactorNumberAlreadyExist = new("المصروف بهذا الفاتوره موجود بالفعل", "Expense_With_This_FactorNumber_Already_Exist");
    public static Error ExpenseNotFound = new("لم يتم العثور على المصروف", "Expense_Not_Found");
    public static Error YouAreNotAllowedToDoThisAction = new("لا يُسمح لك بالقيام بهذا الإجراء", "You_Are_Not_Allowed_To_Do_This_Action");
}