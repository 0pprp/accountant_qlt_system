namespace Application.OrderLists.Commands.Admin.Update;

public class UpdateOrderListCommandValidator : AbstractValidator<UpdateOrderListCommand>
{
    public UpdateOrderListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الإسم فارغًا.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز اسم قائمة 50 حرف");
    }
}