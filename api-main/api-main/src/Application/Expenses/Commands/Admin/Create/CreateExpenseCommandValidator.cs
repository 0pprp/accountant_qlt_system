using Application.Common.Extensions;
using Application.Common.Utilities;
using ByteSizeLib;

namespace Application.Expenses.Commands.Admin.Create;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
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
        
        RuleForEach(x => x.Attachments)
            .NotEmpty()
            .WithMessage("لا يمكن أن تكون الصورة فارغة.")
            .Must(x => AttachmentUtility.ImageContentTypes.Contains(x.ContentType))
            .WithMessage("تنسيق الصورة غير صحيح.")
            .Must(x => ByteSize.FromBytes(x.Length).MegaBytes <= 2)
            .WithMessage("لا يمكن أن يتجاوز حجم الصورة 2 میغابایت.")
            .When(x => x.Attachments is not null);
    }
}
