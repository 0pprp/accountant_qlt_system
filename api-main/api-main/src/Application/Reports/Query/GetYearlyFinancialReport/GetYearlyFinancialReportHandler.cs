using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Query.GetYearlyFinancialReport;

public class GetYearlyFinancialReportHandler : IRequestHandler<GetYearlyFinancialReportQuery, Result<GetYearlyFinancialReportResponse>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetYearlyFinancialReportHandler(IApplicationDbContextFactory dbContextFactory, IDateTimeProvider dateTimeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetYearlyFinancialReportResponse>> Handle(GetYearlyFinancialReportQuery request,
        CancellationToken cancellationToken)
    {
        var year = request.Year ?? _dateTimeProvider.Today.Year;

        var sellAmountsByMonthTask = GetSellAmountsByMonthAsync(request.BranchId, year, cancellationToken);
        var purchaseAmountsByMonthTask = GetPurchaseAmountsByMonthAsync(request.BranchId, year, cancellationToken);
        var installmentPaymentAmountsByMonthTask = GetInstallmentPaymentAmountsByMonthAsync(request.BranchId, year, cancellationToken);

        await Task.WhenAll(sellAmountsByMonthTask, purchaseAmountsByMonthTask, installmentPaymentAmountsByMonthTask);

        var sellAmountsByMonth = await sellAmountsByMonthTask;
        var purchaseAmountsByMonth = await purchaseAmountsByMonthTask;
        var installmentPaymentAmountsByMonth = await installmentPaymentAmountsByMonthTask;

        return new GetYearlyFinancialReportResponse
        {
            SellAmounts = BuildMonthlyReport(sellAmountsByMonth),
            PurchaseAmounts = BuildMonthlyReport(purchaseAmountsByMonth),
            InstallmentPaymentAmounts = BuildMonthlyReport(installmentPaymentAmountsByMonth),
            TotalSellAmount = sellAmountsByMonth.Values.Sum(),
            TotalPurchaseAmount = purchaseAmountsByMonth.Values.Sum(),
            TotalInstallmentPaymentAmount = installmentPaymentAmountsByMonth.Values.Sum()
        };
    }

    private async Task<Dictionary<int, double>> GetSellAmountsByMonthAsync(int branchId, int year, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Orders.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .Where(x => x.Step == OrderStep.Completed)
            .Where(x => x.SaleDate!.Value.Year == year)
            .GroupBy(x => x.SaleDate!.Value.Month)
            .Select(x => new
            {
                Month = x.Key,
                Value = x.Sum(o => o.SellAmount)
            })
            .ToDictionaryAsync(x => x.Month, x => x.Value, cancellationToken);
    }

    private async Task<Dictionary<int, double>> GetPurchaseAmountsByMonthAsync(int branchId, int year, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Purchases.AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .Where(x => x.CreatedAt!.Value.Year == year)
            .GroupBy(x => x.CreatedAt!.Value.Month)
            .Select(x => new
            {
                Month = x.Key,
                Value = x.Sum(p => p.TotalAmount)
            })
            .ToDictionaryAsync(x => x.Month, x => x.Value, cancellationToken);
    }

    private async Task<Dictionary<int, double>> GetInstallmentPaymentAmountsByMonthAsync(int branchId, int year, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.BranchId == branchId)
            .Where(x => x.Date.Year == year)
            .GroupBy(x => x.Date.Month)
            .Select(x => new
            {
                Month = x.Key,
                Value = x.Sum(p => p.Amount)
            })
            .ToDictionaryAsync(x => x.Month, x => x.Value, cancellationToken);
    }

    private static List<MonthlyFinancialValue> BuildMonthlyReport(IReadOnlyDictionary<int, double> amountsByMonth)
    {
        return Enumerable.Range(1, 12)
            .Select(month => new MonthlyFinancialValue
            {
                Month = month,
                Value = amountsByMonth.GetValueOrDefault(month, 0)
            })
            .ToList();
    }
}
