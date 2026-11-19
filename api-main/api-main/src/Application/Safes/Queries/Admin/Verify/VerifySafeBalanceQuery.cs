namespace Application.Safes.Queries.Admin.Verify;

/// <summary>تحقق حسابي: الرصيد المخزون مقابل مجموع الحركات.</summary>
public record VerifySafeBalanceQuery(int SafeId) : IRequest<Result<VerifySafeBalanceResponse>>;
