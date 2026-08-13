namespace Application.Branches.Commands.Admin.Update;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم الفرع مطلوب")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم الفرع 50 حرف");
    }
}