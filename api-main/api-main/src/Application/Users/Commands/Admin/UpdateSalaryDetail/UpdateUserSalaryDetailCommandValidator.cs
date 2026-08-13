using Domain.Entities.UserAggregate.Enums;

namespace Application.Users.Commands.Admin.UpdateSalaryDetail;

public class UpdateUserSalaryDetailCommandValidator : AbstractValidator<UpdateUserSalaryDetailCommand>
{
    public UpdateUserSalaryDetailCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.SaleSharePercent is null && x.InstallmentSharePercent is null)
            .When(x => x.Type == SalaryType.Fixed)
            .WithMessage("في راتب كامل، لا يمكن أن تحتوي نسب المبيعات و الواردات على قيم.");
        
        RuleFor(x => x)
            .Must(x => x.SaleSharePercent is not null && x.InstallmentSharePercent is not null)
            .When(x => x.Type == SalaryType.CommissionBased)
            .WithMessage("في نوع راتب بالنسية، لا يمكن ترك نسبة المبيعات و الواردات فارغة.");

        RuleFor(x => x.SaleSharePercent)
            .InclusiveBetween(0, 100)
            .WithMessage("يجب أن تكون نسبة المبيعات بين 0 و 100.");
        
        RuleFor(x => x.InstallmentSharePercent)
            .InclusiveBetween(0, 100)
            .WithMessage("يجب أن تكون نسبة الواردات بين 0 و 100.");
    }
}