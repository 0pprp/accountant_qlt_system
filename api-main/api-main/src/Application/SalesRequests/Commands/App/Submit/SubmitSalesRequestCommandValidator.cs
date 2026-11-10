using FluentValidation;

namespace Application.SalesRequests.Commands.App.Submit;

public class SubmitSalesRequestCommandValidator : AbstractValidator<SubmitSalesRequestCommand>
{
    public SubmitSalesRequestCommandValidator()
    {
        RuleFor(x => x.SubmissionKey).NotEmpty();
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CustomerPhone).NotEmpty().Must(SalesRequestSubmissionRules.ValidPhone)
            .WithMessage("رقم الهاتف يجب أن يبدأ بـ07 ويتكون من 11 رقمًا");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RequestedProduct).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
