using Application.Common.Extensions;

namespace Application.Expenses.Commands.Admin.Update;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseItems)
            .NotEmpty()
            .WithMessage("منتجات مطلوب");

        RuleForEach(x => x.ExpenseItems)
            .ChildRules(x =>
            {
                x.RuleFor(dto => dto.Name)
                    .NotEmpty()
                    .WithMessage("اسم العنصر مطلوب")
                    .MaximumLength(100)
                    .WithMessage("يجب ألا يتجاوز اسم العنصر 100 حرف");

                x.RuleFor(dto => dto.Quantity)
                    .GreaterThan(0)
                    .WithMessage("الكمية يجب أن تكون أكبر من صفر")
                    .When(dto => dto.Quantity is not null);

                x.RuleFor(dto => dto.Amount)
                    .GreaterThan(0)
                    .WithMessage("المبلغ يجب أن يكون أكبر من صفر")
                    .MustBeDivisibleBy1000();
            });
    }
}
