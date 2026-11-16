using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Queries.App.GetPaginated;

public class GetPaginatedInstallmentPaymentsHandler : IRequestHandler<GetPaginatedInstallmentPaymentsQuery,
    Result<List<GetPaginatedInstallmentPaymentsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetPaginatedInstallmentPaymentsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<GetPaginatedInstallmentPaymentsResponse>>> Handle(GetPaginatedInstallmentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var businessDate = DateOnly.FromDateTime(_dateTimeProvider.UtcNow); // 00:00 UTC = 03:00 Baghdad
        var from = request.Filter.StartDate ?? businessDate;
        var to = request.Filter.EndDate ?? businessDate;

        var installmentPayments = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.OrderList!.MandobId == request.UserId || x.Order.OrderList.MotabaId == request.UserId)
            .Where(x => x.Date >= from)
            .Where(x => x.Date <= to)
            .When(request.Filter.FullName is not null, x => x.Order!.Customer!.FullName.Contains(request.Filter.FullName!))
            .When(request.Filter.OrderListId is not null, x => x.Order!.OrderListId == request.Filter.OrderListId)
            .Select(x => new GetPaginatedInstallmentPaymentsResponse
            {
                Id = x.Id,
                CustomerId = x.Order!.CustomerId,
                CustomerFullName = x.Order!.Customer!.FullName,
                Amount = x.Amount,
                Date = x.Date,
                LastUpdatedAt = x.LastUpdatedAt!.Value,
                OrderId = x.OrderId,
                OrderListId = x.Order.OrderListId!.Value
            })
            .KeysetPaginateAsync(key: x => x.LastUpdatedAt, pagination: request.Pagination, cancellationToken: cancellationToken);

        return installmentPayments;
    }
}