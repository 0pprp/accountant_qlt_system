namespace Application.OrderLists.Commands.Admin.Create;

public class CreateOrderListCommandValidator : AbstractValidator<CreateOrderListCommand>
{
    public CreateOrderListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الإسم فارغًا.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم قائمة 50 حرف");
    }
}