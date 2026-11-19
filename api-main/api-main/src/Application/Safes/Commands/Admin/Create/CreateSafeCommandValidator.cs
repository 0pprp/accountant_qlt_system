using FluentValidation;

namespace Application.Safes.Commands.Admin.Create;

public class CreateSafeCommandValidator : AbstractValidator<CreateSafeCommand>
{
    public CreateSafeCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("يجب تحديد الفرع");

        RuleFor(x => x)
            .Must(x => x.OrderListId is not null || x.UserId is not null)
            .WithMessage("يجب ربط الخزينة بقائمة أو بموظف");

        RuleFor(x => x)
            .Must(x => x.OrderListId is null || x.UserId is null)
            .WithMessage("لا يمكن ربط الخزينة بقائمة وموظف في الوقت نفسه");
    }
}
