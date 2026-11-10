using Application.Common.Interfaces;
using Application.Committee.Common;
using Application.FollowerWork.Common;
using Domain.Entities.FollowerWorkAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.AuthenticatePendingSale;

public record AuthenticateCommitteePendingSaleCommand(
    int UserId,
    int OrderId,
    int Rating,
    string? Note,
    decimal ShopStockEstimatedValue,
    decimal EstimatedDailyRevenue) : IRequest<Result>;

public class AuthenticateCommitteePendingSaleHandler
    : IRequestHandler<AuthenticateCommitteePendingSaleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public AuthenticateCommitteePendingSaleHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result> Handle(
        AuthenticateCommitteePendingSaleCommand request,
        CancellationToken cancellationToken)
    {
        if (FollowerSaleAuthRules.IsValidRating(request.Rating) == false)
            return Result.Failed(new Error("التقييم يجب أن يكون من 1 إلى 10", "FollowerSaleAuth.InvalidRating"));

        if (FollowerSaleAuthRules.IsValidNoteForRating(request.Rating, request.Note) == false)
            return Result.Failed(new Error("الملاحظة إجبارية عندما يكون التقييم أقل من 7", "FollowerSaleAuth.NoteRequired"));

        if (FollowerSaleAuthRules.IsValidShopMoney(request.ShopStockEstimatedValue) == false)
            return Result.Failed(new Error("قيمة بضاعة المحل مطلوبة ويجب أن تكون أكبر من صفر", "FollowerSaleAuth.StockRequired"));

        if (FollowerSaleAuthRules.IsValidShopMoney(request.EstimatedDailyRevenue) == false)
            return Result.Failed(new Error("قيمة الوارد اليومي مطلوبة ويجب أن تكون أكبر من صفر", "FollowerSaleAuth.RevenueRequired"));

        var order = await _db.Orders
            .Include(o => o.OrderList)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order?.OrderList is null || order.OrderListId is null)
            return CommitteeErrors.ListNotFound;

        // الكتابة تتطلب تكليف Motaba الحالي على القائمة — ليس Mandob
        if (order.OrderList.MotabaId != request.UserId)
            return CommitteeErrors.ListNotFound;

        var already = await _db.FollowerSaleAuthentications.AsNoTracking()
            .AnyAsync(a => a.OrderId == request.OrderId, cancellationToken);
        if (already)
            return Result.Failed(new Error("تمت مصادقة هذا المبيع مسبقاً", "FollowerSaleAuth.AlreadyAuthenticated"));

        if (order.ExecutionStatus != OrderExecutionStatus.NotStarted)
            return Result.Failed(new Error("الطلب ليس بانتظار التصديق", "Committee_Sale_Not_Pending"));

        var followerName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var auth = new FollowerSaleAuthentication(
            order.Id,
            order.CustomerId,
            order.OrderListId.Value,
            order.BranchId,
            request.UserId,
            string.IsNullOrWhiteSpace(followerName) ? null : followerName.Trim(),
            request.Rating,
            FollowerSaleAuthRules.NormalizeNote(request.Note),
            decimal.Round(request.ShopStockEstimatedValue, 0, MidpointRounding.AwayFromZero),
            decimal.Round(request.EstimatedDailyRevenue, 0, MidpointRounding.AwayFromZero),
            _clock.UtcNowOffset);

        _db.FollowerSaleAuthentications.Add(auth);

        // أثر تنفيذ الطلب كما مسار اللجنة الحالي — منفصل عن سجل المصادقة كمصدر حقيقة اللوحة
        order.Approve();

        // إغلاق تأجيل الدفعة 06: توجيه مبيع خاطئ ≤6 إلى صندوق منفصل (ليس رفض SalesRequest)
        if (FollowerSaleAuthRules.IsWrongSaleRating(request.Rating))
        {
            var existsNotice = await _db.WrongSaleNotices.AsNoTracking()
                .AnyAsync(x => x.OrderId == order.Id, cancellationToken);
            if (existsNotice == false)
            {
                var customer = await _db.Customers.AsNoTracking()
                    .Where(c => c.Id == order.CustomerId)
                    .Select(c => new { c.FullName, c.PhoneNumber })
                    .FirstOrDefaultAsync(cancellationToken);
                var reason = FollowerSaleAuthRules.FormatWrongSaleReason(
                    request.Rating,
                    request.Note,
                    request.ShopStockEstimatedValue,
                    request.EstimatedDailyRevenue,
                    customer?.FullName,
                    customer?.PhoneNumber);
                _db.WrongSaleNotices.Add(new WrongSaleNotice(
                    order.Id,
                    order.CustomerId,
                    order.BranchId,
                    request.UserId,
                    string.IsNullOrWhiteSpace(followerName) ? null : followerName.Trim(),
                    customer?.FullName,
                    customer?.PhoneNumber,
                    request.Rating,
                    FollowerSaleAuthRules.NormalizeNote(request.Note),
                    decimal.Round(request.ShopStockEstimatedValue, 0, MidpointRounding.AwayFromZero),
                    decimal.Round(request.EstimatedDailyRevenue, 0, MidpointRounding.AwayFromZero),
                    reason,
                    _clock.UtcNowOffset));
            }
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failed(new Error("تمت مصادقة هذا المبيع مسبقاً", "FollowerSaleAuth.AlreadyAuthenticated"));
        }

        return Result.Success();
    }
}
