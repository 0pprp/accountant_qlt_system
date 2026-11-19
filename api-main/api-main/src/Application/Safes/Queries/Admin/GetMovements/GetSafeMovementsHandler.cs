using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Safes.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetMovements;

public class GetSafeMovementsHandler
    : IRequestHandler<GetSafeMovementsQuery, Result<PaginatedList<GetSafeMovementsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetSafeMovementsHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<GetSafeMovementsResponse>>> Handle(
        GetSafeMovementsQuery request, CancellationToken cancellationToken)
    {
        var scope = await SafeAccess.EnsureSafeInScopeAsync(
            _dbContext, _currentUser, request.SafeId, cancellationToken);
        if (scope.IsFailed)
            return scope.Error;

        var fromUtc = request.Filter.StartDate is DateOnly start
            ? new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue).AddHours(-3), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        var toUtc = request.Filter.EndDate is DateOnly end
            ? new DateTimeOffset(end.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-3), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        var page = await _dbContext.SafeMovements.AsNoTracking()
            .Where(x => x.SafeId == request.SafeId)
            .When(request.Filter.Kind is not null, x => x.Kind == request.Filter.Kind)
            .When(fromUtc is not null, x => x.CreatedAt >= fromUtc)
            .When(toUtc is not null, x => x.CreatedAt < toUtc)
            .When(request.Filter.SearchTerm is not null, x => x.Note != null && x.Note.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetSafeMovementsResponse
            {
                Id = x.Id,
                Kind = x.Kind,
                Amount = x.Amount,
                BalanceAfter = x.BalanceAfter,
                Note = x.Note,
                CounterpartySafeId = x.CounterpartySafeId,
                CounterpartySafeName = x.CounterpartySafe != null ? x.CounterpartySafe.Name : null,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByFullName = _dbContext.Users
                    .Where(u => u.Id == x.PerformedByUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefault(),
                CreatedAt = x.CreatedAt,
                InstallmentPaymentId = x.InstallmentPaymentId,
                CustomerFullName = x.InstallmentPaymentId != null
                    ? _dbContext.Orders.Where(o => o.Id == x.OrderId).Select(o => o.Customer!.FullName).FirstOrDefault()
                    : null
            })
            .SortOrDefault(request.SortCriteria, x => x.Id)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return page;
    }
}
