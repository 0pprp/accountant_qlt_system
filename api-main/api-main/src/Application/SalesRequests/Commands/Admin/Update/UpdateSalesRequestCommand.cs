using Application.SalesRequests.Commands.App.Submit;

namespace Application.SalesRequests.Commands.Admin.Update;

public record UpdateSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public int BranchId { get; init; }
    public int? ProvinceId { get; init; }
    /// <summary>إن وُجد ويختلف عن المسند الحالي → إعادة إسناد (يشمل فرع/محافظة أخرى).</summary>
    public int? EmployeeId { get; init; }
    public string? Reason { get; init; }
}

public class UpdateSalesRequestCommandValidator : AbstractValidator<UpdateSalesRequestCommand>
{
    public UpdateSalesRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .Must(SalesRequestSubmissionRules.ValidPhone)
            .WithMessage("رقم الهاتف يجب أن يبدأ بـ07 ويتكون من 11 رقمًا");
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.RequestedProduct).MaximumLength(200);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProvinceId).GreaterThan(0).When(x => x.ProvinceId.HasValue);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId.HasValue);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
