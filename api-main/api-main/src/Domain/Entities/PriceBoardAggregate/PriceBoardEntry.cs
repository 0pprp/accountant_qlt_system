using Domain.Common;

namespace Domain.Entities.PriceBoardAggregate;

/// <summary>
/// إعلان أسعار للمراقبة حسب المحافظة — معلوماتي فقط؛ لا يعدّل Product/Order.
/// </summary>
public class PriceBoardEntry : Entity
{
    private PriceBoardEntry()
    {
    }

    public PriceBoardEntry(
        string productName,
        decimal salePrice,
        decimal dailyInstallment,
        decimal downPayment,
        int targetProvinceId,
        string targetProvinceName,
        int createdByUserId,
        string? createdByName,
        DateTimeOffset createdAtUtc)
    {
        ApplyValues(productName, salePrice, dailyInstallment, downPayment, targetProvinceId, targetProvinceName);
        CreatedByUserId = createdByUserId;
        CreatedByName = createdByName;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
    }

    public string ProductName { get; private set; } = "";
    public decimal SalePrice { get; private set; }
    public decimal DailyInstallment { get; private set; }
    public decimal DownPayment { get; private set; }
    public int TargetProvinceId { get; private set; }
    public string TargetProvinceName { get; private set; } = "";
    public int CreatedByUserId { get; private set; }
    public string? CreatedByName { get; private set; }
    public int? UpdatedByUserId { get; private set; }
    public string? UpdatedByName { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Update(
        string productName,
        decimal salePrice,
        decimal dailyInstallment,
        decimal downPayment,
        int targetProvinceId,
        string targetProvinceName,
        int actorId,
        string? actorName,
        int expectedVersion,
        DateTimeOffset at)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        ApplyValues(productName, salePrice, dailyInstallment, downPayment, targetProvinceId, targetProvinceName);
        UpdatedByUserId = actorId;
        UpdatedByName = actorName;
        UpdatedAtUtc = at;
        ConcurrencyStamp++;
    }

    private void ApplyValues(
        string productName,
        decimal salePrice,
        decimal dailyInstallment,
        decimal downPayment,
        int targetProvinceId,
        string targetProvinceName)
    {
        var name = (productName ?? "").Trim();
        if (name.Length is < 1 or > 400)
            throw new InvalidOperationException("اسم المنتج مطلوب وبحد أقصى 400 حرف.");
        if (targetProvinceId <= 0)
            throw new InvalidOperationException("المحافظة مطلوبة.");
        if (salePrice < 0 || dailyInstallment < 0 || downPayment < 0)
            throw new InvalidOperationException("المبالغ لا تكون سالبة.");

        ProductName = name;
        SalePrice = salePrice;
        DailyInstallment = dailyInstallment;
        DownPayment = downPayment;
        TargetProvinceId = targetProvinceId;
        TargetProvinceName = (targetProvinceName ?? "").Trim();
        if (TargetProvinceName.Length == 0)
            TargetProvinceName = $"#{targetProvinceId}";
    }
}
