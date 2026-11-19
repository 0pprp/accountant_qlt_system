using Domain.Entities.SafeAggregate.Enums;

namespace Application.Safes.Queries.Admin.GetMovements;

public record GetSafeMovementsResponse
{
    public int Id { get; set; }
    public SafeMovementKind Kind { get; set; }

    /// <summary>المبلغ الموقّع — موجب داخل، سالب خارج.</summary>
    public double Amount { get; set; }

    public double BalanceAfter { get; set; }
    public string? Note { get; set; }
    public int? CounterpartySafeId { get; set; }
    public string? CounterpartySafeName { get; set; }
    public int PerformedByUserId { get; set; }
    public string? PerformedByFullName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>التسديد الذي أنشأ حركة التحصيل أو عكسها.</summary>
    public int? InstallmentPaymentId { get; set; }

    public string? CustomerFullName { get; set; }
}
