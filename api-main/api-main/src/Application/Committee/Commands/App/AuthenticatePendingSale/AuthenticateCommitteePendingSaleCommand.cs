using Application.Common.Interfaces;
using Application.Committee.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.AuthenticatePendingSale;

public record AuthenticateCommitteePendingSaleCommand(int UserId, int OrderId) : IRequest<Result>;

public class AuthenticateCommitteePendingSaleHandler
    : IRequestHandler<AuthenticateCommitteePendingSaleCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AuthenticateCommitteePendingSaleHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(AuthenticateCommitteePendingSaleCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.OrderList)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order?.OrderList is null)
            return CommitteeErrors.ListNotFound;

        if (order.OrderList.MotabaId != request.UserId && order.OrderList.MandobId != request.UserId)
            return CommitteeErrors.ListNotFound;

        if (order.ExecutionStatus != OrderExecutionStatus.NotStarted)
            return Result.Failed(new Error("الطلب ليس بانتظار التصديق", "Committee_Sale_Not_Pending"));

        // تصديق مسؤول اللجنة: اعتماد الطلب وبدء التنفيذ (نفس نية Followers/PendingSales/Authenticate)
        order.Approve();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
