namespace Application.SalesRatings.Common;

public static class SalesRatingErrors
{
    public static Error InvalidLevel = new("المستوى يجب أن يكون ضعيف أو متلكئ أو سيء.", "Sales_Rating_Invalid_Level");
    public static Error CustomerNotFound = new("الزبون غير موجود ضمن مبيعاتك.", "Sales_Rating_Customer_Not_Found");
}
