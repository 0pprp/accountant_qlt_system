namespace Application.SalesRequests.Commands.Admin.DeleteSoldSale;

public record DeleteSoldSaleCommand : IRequest<Result<DeleteSoldSaleImpact>>
{
    public int RequestId { get; init; }
    public required string Reason { get; init; }
    public bool ConfirmPayments { get; init; }
}

public record GetDeleteSoldSaleImpactQuery(int RequestId) : IRequest<Result<DeleteSoldSaleImpact>>;

public sealed record DeleteSoldSaleImpactItem(string Name, int Quantity, bool ReturnsToStock);

public sealed record DeleteSoldSaleImpact(
    int RequestId,
    int OrderId,
    string CustomerName,
    string? CustomerPhone,
    DateOnly? SaleDate,
    double SellAmount,
    double PrepaymentAmount,
    double PaidTotal,
    int PaymentsCount,
    double ForeignPurchaseRefund,
    int ArchiveRecords,
    IReadOnlyList<DeleteSoldSaleImpactItem> Items);

public class DeleteSoldSaleCommandValidator : AbstractValidator<DeleteSoldSaleCommand>
{
    public DeleteSoldSaleCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("سبب حذف المبيع مطلوب")
            .MinimumLength(5).WithMessage("اكتب سبباً واضحاً (5 أحرف على الأقل)")
            .MaximumLength(500);
    }
}
