namespace Application.ProductCategories.Commands.Admin.Create;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم الفئة مطلوب")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم الفئة 50 حرف");
    }
}