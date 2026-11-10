namespace Application.BranchManagerAccounts.Common;

public sealed record BranchManagerAccountDto(
    int Id,
    string Username,
    string FullName,
    bool IsActive,
    IReadOnlyList<int> BranchIds,
    IReadOnlyList<string> BranchNames);

public sealed record BranchManagerAccountListResponse(
    IReadOnlyList<BranchManagerAccountDto> Items,
    int TotalCount);

public sealed record BranchOptionDto(int Id, string Name);

public static class BranchManagerAccountErrors
{
    public static readonly Error Forbidden = new("لا صلاحية لإدارة حسابات الفرع.", "BranchManagerAccount.Forbidden");
    public static readonly Error NotFound = new("الحساب غير موجود.", "BranchManagerAccount.NotFound");
    public static readonly Error BranchScope = new("فروع خارج نطاقك.", "BranchManagerAccount.BranchScope");
    public static readonly Error Validation = new("بيانات الحساب غير صالحة.", "BranchManagerAccount.Validation");
    public static readonly Error UsernameTaken = new("اسم المستخدم مستخدم.", "BranchManagerAccount.UsernameTaken");
    public static readonly Error RoleForbidden = new("الدور غير مسموح لهذه السياسة.", "BranchManagerAccount.RoleForbidden");
    public static readonly Error Protected = new("حساب محمي.", "BranchManagerAccount.Protected");
}
