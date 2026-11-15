using Application.Common;

namespace Application.ManagementSalesMap.Common;

public static class ManagementSalesMapErrors
{
    public static Error Forbidden = new("غير مصرح بقراءة خريطة المبيعات", "SalesMap_Forbidden");
    public static Error ProvinceForbidden = new("المحافظة خارج نطاق حسابك", "SalesMap_Province_Forbidden");
    public static Error ProvinceNotFound = new("المحافظة غير موجودة", "SalesMap_Province_NotFound");
    public static Error OrderNotFound = new("المبيع غير موجود أو خارج النطاق", "SalesMap_Order_NotFound");
}
