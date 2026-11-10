namespace Application.ManagementTasks.Commands.Admin.Create;

public class CreateManagementTaskCommandValidator : AbstractValidator<CreateManagementTaskCommand>
{
    public CreateManagementTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان المهمة مطلوب")
            .MaximumLength(200).WithMessage("عنوان المهمة يجب ألا يتجاوز 200 حرف");
        RuleFor(x => x.Description)
            .MaximumLength(8000).WithMessage("الوصف يجب ألا يتجاوز 8000 حرف");
        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("أولوية غير صالحة.");
        RuleFor(x => x.DueDate)
            .NotEqual(default(DateOnly)).WithMessage("تاريخ الاستحقاق مطلوب.");
        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("الفرع مطلوب");
        RuleFor(x => x.AssigneeUserId)
            .GreaterThan(0).WithMessage("المكلَّف مطلوب");
    }
}
