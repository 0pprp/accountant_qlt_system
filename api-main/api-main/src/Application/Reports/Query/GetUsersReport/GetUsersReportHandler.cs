using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Query.GetUsersReport;

public class GetUsersReportHandler : IRequestHandler<GetUsersReportQuery, Result<GetUsersReportResponse>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersReportHandler(
        IApplicationDbContextFactory dbContextFactory,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _dbContextFactory = dbContextFactory;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetUsersReportResponse>> Handle(GetUsersReportQuery request, CancellationToken cancellationToken)
    {
        var accessDenied = Application.Reports.Common.ReportBranchAccess.DenyIfOutOfScope(
            _currentUserService, request.BranchId);
        if (accessDenied is not null)
            return accessDenied;

        var previousMonthEnd = new DateOnly(_dateTimeProvider.Today.Year, _dateTimeProvider.Today.Month, 1).AddDays(-1);

        var currentCustomerCountTask = GetCustomerCountAsync(request.BranchId, countUpToDate: null, cancellationToken);
        var previousCustomerCountTask = GetCustomerCountAsync(request.BranchId, countUpToDate: previousMonthEnd, cancellationToken);
        var currentMandobCountTask = GetMandobUserCountAsync(request.BranchId, countUpToDate: null, cancellationToken);
        var previousMandobCountTask = GetMandobUserCountAsync(request.BranchId, countUpToDate: previousMonthEnd, cancellationToken);
        var currentAllUserCountTask = GetAllUserCountAsync(request.BranchId, countUpToDate: null, cancellationToken);
        var previousAllUserCountTask = GetAllUserCountAsync(request.BranchId, countUpToDate: previousMonthEnd, cancellationToken);

        await Task.WhenAll(
            currentCustomerCountTask,
            previousCustomerCountTask,
            currentMandobCountTask,
            previousMandobCountTask,
            currentAllUserCountTask,
            previousAllUserCountTask);

        var currentCustomerCount = await currentCustomerCountTask;
        var previousCustomerCount = await previousCustomerCountTask;
        var currentMandobCount = await currentMandobCountTask;
        var previousMandobCount = await previousMandobCountTask;
        var currentAllUserCount = await currentAllUserCountTask;
        var previousAllUserCount = await previousAllUserCountTask;

        return new GetUsersReportResponse
        {
            Customers = new CountMetric
            {
                Count = currentCustomerCount,
                ChangePercent = CalculateChangePercent(currentCustomerCount, previousCustomerCount)
            },
            MandobUsers = new CountMetric
            {
                Count = currentMandobCount,
                ChangePercent = CalculateChangePercent(currentMandobCount, previousMandobCount)
            },
            AllUsers = new CountMetric
            {
                Count = currentAllUserCount,
                ChangePercent = CalculateChangePercent(currentAllUserCount, previousAllUserCount)
            }
        };
    }

    private async Task<int> GetCustomerCountAsync(int branchId, DateOnly? countUpToDate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Customers.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .When(countUpToDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= countUpToDate)
            .CountAsync(cancellationToken);
    }

    private async Task<int> GetMandobUserCountAsync(int branchId, DateOnly? countUpToDate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Users.AsNoTracking()
            .Where(x => x.UserBranches.Any(ub => ub.BranchId == branchId))
            .Where(x => x.UserRoles.Any(ur => ur.Role!.Name == Application.Common.SeedData.Roles.Mandob.Name))
            .When(countUpToDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= countUpToDate)
            .CountAsync(cancellationToken);
    }

    private async Task<int> GetAllUserCountAsync(int branchId, DateOnly? countUpToDate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Users.AsNoTracking()
            .Where(x => x.UserBranches.Any(ub => ub.BranchId == branchId))
            .When(countUpToDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= countUpToDate)
            .CountAsync(cancellationToken);
    }

    private static double CalculateChangePercent(int current, int previous)
    {
        if (previous == 0)
            return current == 0 ? 0 : 100;

        return Math.Round((current - previous) / (double)previous * 100, 1);
    }
}
