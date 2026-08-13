using System.Linq.Expressions;
using Application.Common.Extensions;
using Domain.Entities.UserAggregate;

namespace Application.Users.Common;

public record UserColumn(
    string Key,
    string DisplayName,
    string DataType,
    Expression<Func<User, object?>> Selector);

public static class UserColumns
{
    public static readonly List<UserColumn> All =
    [
        new("Id", "المعرف", "number", x => x.Id),
        new("FullName", "الاسم الكامل", "string", x => x.FullName),
        new("MotherName", "اسم الأم", "string", x => x.MotherName),
        new("Username", "اسم المستخدم", "string", x => x.Username),
        new("NationalCode", "الرقم الوطني", "string", x => x.NationalCode),
        new("BirthDate", "تاريخ الميلاد", "date", x => x.BirthDate),
        new("PhoneNumber", "رقم الهاتف", "string", x => x.PhoneNumber),
        new("Address", "العنوان", "string", x => x.Address),
        new("BranchNames", "الفروع", "string",
            x => string.Join(", ", x.UserBranches.Select(ub => ub.Branch!.Name))),
        new("RoleNames", "الأدوار", "string",
            x => string.Join(", ", x.UserRoles.Select(ur => ur.Role!.Name))),
        new("CreationStep", "خطوة الإنشاء", "string", x => x.CreationStep.ToArabicString()),
        new("UndeliveredCashAmount", "المبلغ النقدي غير المسلّم", "number", x => x.UndeliveredCashAmount),
        new("SalaryType", "نوع الراتب", "string",
            x => x.SalaryDetail.Type != null ? x.SalaryDetail.Type.Value.ToArabicString() : null),
        new("SalaryAmount", "مبلغ الراتب", "number", x => x.SalaryDetail.Amount),
        new("SaleSharePercent", "نسبة حصة البيع", "number", x => x.SalaryDetail.SaleSharePercent),
        new("InstallmentSharePercent", "نسبة حصة الأقساط", "number", x => x.SalaryDetail.InstallmentSharePercent),
        new("CreatedAt", "تاريخ الإنشاء", "datetime", x => x.CreatedAt!.Value)
    ];

    public static IEnumerable<string> GetAllKeys() => All.Select(c => c.Key);

    public static UserColumn? GetField(string key) =>
        All.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}
