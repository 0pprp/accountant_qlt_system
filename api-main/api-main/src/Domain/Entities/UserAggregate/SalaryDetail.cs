using Domain.Entities.UserAggregate.Enums;

namespace Domain.Entities.UserAggregate;

public record SalaryDetail
{
    public SalaryDetail() { }
    public SalaryDetail(SalaryType type, double amount, double? saleSharePercent, double? installmentSharePercent)
    {
        Type = type;
        Amount = amount;
        SaleSharePercent = saleSharePercent;
        InstallmentSharePercent = installmentSharePercent;
    }

    public SalaryType? Type { get; set; }
    public double? Amount { get; set; }
    public double? SaleSharePercent { get; set; }
    public double? InstallmentSharePercent { get; set; }

    public void Update(SalaryType type, double amount, double? saleSharePercent, double? installmentSharePercent)
    {
        Type = type;
        Amount = amount;
        SaleSharePercent = saleSharePercent;
        InstallmentSharePercent = installmentSharePercent;
    }
}