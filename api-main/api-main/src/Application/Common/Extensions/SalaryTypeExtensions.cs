using Domain.Entities.UserAggregate.Enums;

namespace Application.Common.Extensions;

public static class SalaryTypeExtensions
{
    public static string ToArabicString(this SalaryType salaryType)
    {
        return salaryType switch
        {
            SalaryType.Fixed => "ثابت",
            SalaryType.CommissionBased => "عمولة",
            _ => throw new ArgumentOutOfRangeException(nameof(salaryType), salaryType, null)
        };
    }
}
