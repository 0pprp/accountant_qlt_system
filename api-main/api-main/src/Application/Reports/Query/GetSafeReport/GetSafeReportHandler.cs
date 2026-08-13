using Application.Common.Interfaces;
using Application.Common.Utilities;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Query.GetSafeReport;

public class GetSafeReportHandler : IRequestHandler<GetSafeReportQuery, Result<GetSafeReportResponse>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSafeReportHandler(IApplicationDbContextFactory dbContextFactory, IDateTimeProvider dateTimeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetSafeReportResponse>> Handle(GetSafeReportQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;
        var yesterday = today.AddDays(-1);
        var thisWeekStart = today.GetWeekStartDate();
        var lastWeekStart = thisWeekStart.AddDays(-7);
        var lastWeekEnd = thisWeekStart.AddDays(-1);
        var firstDayOfCurrentMonth = new DateOnly(today.Year, today.Month, 1);
        var lastMonthEnd = firstDayOfCurrentMonth.AddDays(-1);
        var lastMonthStart = new DateOnly(lastMonthEnd.Year, lastMonthEnd.Month, 1);
        var lastYear = today.Year - 1;
        var lastYearStart = new DateOnly(lastYear, 1, 1);
        var lastYearEnd = new DateOnly(lastYear, 12, 31);

        var todayMetricsTask = GetSafeMetricsAsync(request.BranchId, today, today, cancellationToken);
        var yesterdayMetricsTask = GetSafeMetricsAsync(request.BranchId, yesterday, yesterday, cancellationToken);
        var lastWeekMetricsTask = GetSafeMetricsAsync(request.BranchId, lastWeekStart, lastWeekEnd, cancellationToken);
        var lastMonthMetricsTask = GetSafeMetricsAsync(request.BranchId, lastMonthStart, lastMonthEnd, cancellationToken);
        var lastYearMetricsTask = GetSafeMetricsAsync(request.BranchId, lastYearStart, lastYearEnd, cancellationToken);

        await Task.WhenAll(
            todayMetricsTask,
            yesterdayMetricsTask,
            lastWeekMetricsTask,
            lastMonthMetricsTask,
            lastYearMetricsTask);

        return new GetSafeReportResponse
        {
            Today = await todayMetricsTask,
            Yesterday = await yesterdayMetricsTask,
            LastWeek = await lastWeekMetricsTask,
            LastMonth = await lastMonthMetricsTask,
            LastYear = await lastYearMetricsTask
        };
    }

    private async Task<SafeReportMetricsDto> GetSafeMetricsAsync(
        int branchId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var totalWithdrawalAmountTask = GetTotalWithdrawalAmountAsync(branchId, startDate, endDate, cancellationToken);
        var totalTransferredAmountTask = GetTotalTransferredAmountAsync(branchId, startDate, endDate, cancellationToken);
        var totalEarnedAmountTask = GetTotalEarnedAmountAsync(branchId, startDate, endDate, cancellationToken);
        var totalInAmountTask = GetTotalInAmountAsync(branchId, endDate, cancellationToken);
        var totalOutAmountTask = GetTotalOutAmountAsync(branchId, endDate, cancellationToken);

        await Task.WhenAll(
            totalWithdrawalAmountTask,
            totalTransferredAmountTask,
            totalEarnedAmountTask,
            totalInAmountTask,
            totalOutAmountTask);

        return new SafeReportMetricsDto
        {
            TotalWithdrawalAmount = await totalWithdrawalAmountTask,
            TotalTransferredAmount = await totalTransferredAmountTask,
            TotalEarnedAmount = await totalEarnedAmountTask,
            RemainingCashAmount = await totalInAmountTask - await totalOutAmountTask
        };
    }

    private async Task<double> GetTotalWithdrawalAmountAsync(
        int branchId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.Safe!.BranchId == branchId)
            .Where(x => x.Direction == TransactionDirection.Out)
            .Where(x => x.Status == TransactionStatus.Approved)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= startDate)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= endDate)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private async Task<double> GetTotalTransferredAmountAsync(
        int branchId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.Safe!.BranchId == branchId)
            .Where(x => x.Type == TransactionType.SafeTransfer)
            .Where(x => x.Direction == TransactionDirection.Out)
            .Where(x => x.Status == TransactionStatus.Approved)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= startDate)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= endDate)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private async Task<double> GetTotalEarnedAmountAsync(
        int branchId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.Safe!.BranchId == branchId)
            .Where(x => x.Direction == TransactionDirection.In)
            .Where(x => x.Status == TransactionStatus.Approved)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= startDate)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= endDate)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private async Task<double> GetTotalInAmountAsync(int branchId, DateOnly endDate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.Safe!.BranchId == branchId)
            .Where(x => x.Direction == TransactionDirection.In)
            .Where(x => x.Status == TransactionStatus.Approved)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= endDate)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private async Task<double> GetTotalOutAmountAsync(int branchId, DateOnly endDate, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Transactions.AsNoTracking()
            .Where(x => x.Safe!.BranchId == branchId)
            .Where(x => x.Direction == TransactionDirection.Out)
            .Where(x => x.Status == TransactionStatus.Approved)
            .Where(x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= endDate)
            .SumAsync(x => x.Amount, cancellationToken);
    }
}