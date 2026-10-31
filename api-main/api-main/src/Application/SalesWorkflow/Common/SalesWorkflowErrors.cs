namespace Application.SalesWorkflow.Common;

public static class SalesWorkflowErrors
{
    public static Error InvalidStatus = new("حالة سير العمل غير صالحة.", "Sales_Workflow_Invalid_Status");
    public static Error CannotProgress = new("لا يمكن تقدم سير العمل لهذه الحالة.", "Sales_Workflow_Cannot_Progress");
    public static Error CannotComplete = new("لا يمكن إكمال سير العمل لهذه الحالة.", "Sales_Workflow_Cannot_Complete");
    public static Error ReasonRequired = new("يجب إدخال سبب عند النقل إلى هذه الحالة.", "Sales_Workflow_Reason_Required");
}
