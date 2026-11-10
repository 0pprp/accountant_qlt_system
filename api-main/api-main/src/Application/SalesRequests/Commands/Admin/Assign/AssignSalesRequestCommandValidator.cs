namespace Application.SalesRequests.Commands.Admin.Assign;

public class AssignSalesRequestCommandValidator : AbstractValidator<AssignSalesRequestCommand>
{
    public AssignSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}
