namespace Application.Branches.Commands.Admin.Create;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم الفرع مطلوب")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم الفرع 50 حرف");
    }
}