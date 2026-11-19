namespace Application.Safes.Commands.Admin.Create;

/// <summary>
/// إنشاء خزينة يدوياً. الربط إلزامي وواحد: إما قائمة أو موظف —
/// خزائن النظام (قاصة الفرع وسجل الصرفيات) تُنشأ تلقائياً ولا تُنشأ من هنا.
/// </summary>
public record CreateSafeCommand : IRequest<Result<CreateSafeResponse>>
{
    public int BranchId { get; set; }
    public int? OrderListId { get; set; }
    public int? UserId { get; set; }
}

public record CreateSafeResponse(int Id);
