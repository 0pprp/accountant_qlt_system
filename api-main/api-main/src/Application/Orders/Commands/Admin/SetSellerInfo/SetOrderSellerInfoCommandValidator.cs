using Application.Common.Interfaces;

namespace Application.Orders.Commands.Admin.SetSellerInfo;

public class SetOrderSellerInfoCommandValidator : AbstractValidator<SetOrderSellerInfoCommand>
{
    public SetOrderSellerInfoCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.CreationAddress)
            .NotEmpty()
            .WithMessage("عنوان البيع مطلوب")
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز عنوان البيع 400 حرف");

        RuleFor(x => x.SaleDate)
            .NotEmpty()
            .WithMessage("تسجيل تاريخ الطلب إلزامي")
            .LessThanOrEqualTo(dateTimeProvider.Today)
            .WithMessage("يجب ألا يتجاوز تاريخ البيع یوم الجاری");
    }
}