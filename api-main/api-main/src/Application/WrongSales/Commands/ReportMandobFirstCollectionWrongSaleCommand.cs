using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Application.WrongSales.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.WrongSales.Commands;

/// <summary>
/// مصدر مندوب الجباية: FirstCollectionEvaluation — تقييم محل ≤6 عند أول تحصيل.
/// يعيد استخدام WrongSaleNotice؛ الكاتب من الجلسة؛ لا يقبل source من العميل.
/// </summary>
public sealed class ReportMandobFirstCollectionWrongSaleCommand
    : IRequest<Result<MandobFirstCollectionEvalResultDto>>
{
    public int CustomerId { get; init; }
    public int ListId { get; init; }
    public int Rating { get; init; }
    public string? Note { get; init; }
    public decimal ShopStockEstimatedValue { get; init; }
    public decimal EstimatedDailyRevenue { get; init; }
    public decimal? SaleAmount { get; init; }
}

public sealed class ReportMandobFirstCollectionWrongSaleHandler
    : IRequestHandler<ReportMandobFirstCollectionWrongSaleCommand, Result<MandobFirstCollectionEvalResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ReportMandobFirstCollectionWrongSaleHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<MandobFirstCollectionEvalResultDto>> Handle(
        ReportMandobFirstCollectionWrongSaleCommand request, CancellationToken ct)
    {
        // صلاحية إنشاء تسديد App للمندوب — ليست ManagePermissions ولا مصدر قابل للانتحال
        if (_current.HasPermission(SeedPermissions.InstallmentPayment.Create) == false)
            return WrongSaleErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WrongSaleErrors.Forbidden;
        if (_current.Role != SeedRoles.Mandob.Name
            && string.Equals(_current.Role, SeedRoles.Mandob.Name, StringComparison.OrdinalIgnoreCase) == false)
        {
            // بعض الجلسات تضع Role كـ DisplayName
            var isMandob = await _db.UserRoles.AsNoTracking()
                .AnyAsync(ur => ur.UserId == actorId && ur.Role!.Name == SeedRoles.Mandob.Name, ct);
            if (isMandob == false)
                return WrongSaleErrors.Forbidden;
        }

        if (FollowerSaleAuthRules.IsValidRating(request.Rating) == false)
            return new Error("التقييم يجب أن يكون من 1 إلى 10", "WrongSale.RatingInvalid");
        if (FollowerSaleAuthRules.IsValidNoteForRating(request.Rating, request.Note) == false)
            return new Error("الملاحظة إجبارية عندما يكون التقييم أقل من 7", "WrongSale.NoteRequired");
        if (FollowerSaleAuthRules.IsValidShopMoney(request.ShopStockEstimatedValue) == false
            || FollowerSaleAuthRules.IsValidShopMoney(request.EstimatedDailyRevenue) == false)
            return new Error("قيم البضاعة والوارد اليومي مطلوبة وأكبر من صفر", "WrongSale.ShopMoneyInvalid");

        var listId = request.ListId > 0 ? request.ListId : 0;
        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == listId && l.MandobId == actorId)
            .Select(l => new { l.Id, l.BranchId })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return new Error("القائمة غير مكلَّفة لهذا المندوب.", "WrongSale.ListNotAssigned");

        var customerOnList = await _db.Orders.AsNoTracking()
            .AnyAsync(o => o.CustomerId == request.CustomerId && o.OrderListId == list.Id, ct);
        if (customerOnList == false)
            return new Error("الزبون ليس على قائمة المندوب.", "WrongSale.CustomerNotOnList");

        // أحدث بيع مكتمل للزبون على القائمة — يربط الإشعار ببيع حقيقي
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId
                        && o.OrderListId == list.Id
                        && o.WorkflowStatus == SalesWorkflowStatus.Sold)
            .OrderByDescending(o => o.SalesCompletedAtUtc ?? o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Select(o => new { o.Id, o.BranchId, o.CustomerId })
            .FirstOrDefaultAsync(ct);
        if (order is null)
            return new Error("لا يوجد بيع مكتمل لهذا الزبون على القائمة.", "WrongSale.OrderNotFound");

        var stock = decimal.Round(request.ShopStockEstimatedValue, 0, MidpointRounding.AwayFromZero);
        var revenue = decimal.Round(request.EstimatedDailyRevenue, 0, MidpointRounding.AwayFromZero);
        var isWrong = FollowerSaleAuthRules.IsWrongSaleRating(request.Rating);

        // تقييم ≥7: نجاح التقييم دون WrongSaleNotice (مطابقة المصدر)
        if (isWrong == false)
        {
            return new MandobFirstCollectionEvalResultDto(
                request.CustomerId, list.Id, request.Rating,
                FollowerSaleAuthRules.NormalizeNote(request.Note),
                stock, revenue, request.SaleAmount,
                IsWrongSale: false, Created: false, NoticeId: null, Notice: null);
        }

        // تكرار: نفس OrderId أو نفس الزبون من مصدر المندوب مسبقاً
        var existing = await _db.WrongSaleNotices.AsNoTracking()
            .Where(n => n.OrderId == order.Id
                        || (n.CustomerId == order.CustomerId
                            && n.SourceApp == WrongSaleNotice.SourceMandobFirstCollection))
            .OrderByDescending(n => n.Id)
            .FirstOrDefaultAsync(ct);
        if (existing is not null)
        {
            var dtoExisting = WrongSaleMapper.ToDto(existing);
            return new MandobFirstCollectionEvalResultDto(
                request.CustomerId, list.Id, request.Rating,
                FollowerSaleAuthRules.NormalizeNote(request.Note),
                stock, revenue, request.SaleAmount,
                IsWrongSale: true, Created: false, NoticeId: dtoExisting.Id, Notice: dtoExisting);
        }

        var customer = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == order.CustomerId)
            .Select(c => new { c.FullName, c.PhoneNumber })
            .FirstOrDefaultAsync(ct);
        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);

        var reason = FollowerSaleAuthRules.FormatWrongSaleReason(
            request.Rating, request.Note, stock, revenue, customer?.FullName, customer?.PhoneNumber);
        if (request.SaleAmount is decimal sa and > 0)
            reason += $"\nمبلغ البيع: {sa:0}";

        var notice = new WrongSaleNotice(
            order.Id,
            order.CustomerId,
            order.BranchId,
            actorId,
            actorName,
            customer?.FullName,
            customer?.PhoneNumber,
            request.Rating,
            FollowerSaleAuthRules.NormalizeNote(request.Note),
            stock,
            revenue,
            reason,
            _clock.UtcNowOffset,
            WrongSaleNotice.SourceMandobFirstCollection);

        _db.WrongSaleNotices.Add(notice);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var raced = await _db.WrongSaleNotices.AsNoTracking()
                .FirstOrDefaultAsync(n => n.OrderId == order.Id, ct);
            if (raced is not null)
            {
                var dtoRaced = WrongSaleMapper.ToDto(raced);
                return new MandobFirstCollectionEvalResultDto(
                    request.CustomerId, list.Id, request.Rating,
                    FollowerSaleAuthRules.NormalizeNote(request.Note),
                    stock, revenue, request.SaleAmount,
                    IsWrongSale: true, Created: false, NoticeId: dtoRaced.Id, Notice: dtoRaced);
            }
            throw;
        }

        var dto = WrongSaleMapper.ToDto(notice);
        return new MandobFirstCollectionEvalResultDto(
            request.CustomerId, list.Id, request.Rating,
            FollowerSaleAuthRules.NormalizeNote(request.Note),
            stock, revenue, request.SaleAmount,
            IsWrongSale: true, Created: true, NoticeId: dto.Id, Notice: dto);
    }
}
