namespace Application.ProductCategories.Commands.Admin.Update;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم الفئة مطلوب")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم الفئة 50 حرف");
    }
}