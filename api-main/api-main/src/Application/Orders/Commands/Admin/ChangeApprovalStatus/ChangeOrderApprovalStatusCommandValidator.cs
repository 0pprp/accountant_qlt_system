using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Commands.Admin.ChangeApprovalStatus;

public class ChangeOrderApprovalStatusCommandValidator : AbstractValidator<ChangeOrderApprovalStatusCommand>
{
    public ChangeOrderApprovalStatusCommandValidator()
    {
        RuleFor(x => x.ApprovalStatus)
            .Must(x => x is OrderApprovalStatus.Approved or OrderApprovalStatus.Rejected)
            .WithMessage("لا يمكن تغيير الحالة إلا إلى موافق عليه أو مرفوض.");
    }
}