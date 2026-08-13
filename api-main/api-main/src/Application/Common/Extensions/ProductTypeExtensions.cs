using Domain.Entities.OrderAggregate.Enums;

namespace Application.Common.Extensions;

public static class ProductTypeExtensions
{
    public static string ToArabicString(this ProductType productType)
    {
        return productType switch
        {
            ProductType.Warehouse => "مبيع من المخزن",
            ProductType.Foreign => "مبيع الخارجي",
            _ => throw new ArgumentOutOfRangeException(nameof(productType), productType, null)
        };
    }
}