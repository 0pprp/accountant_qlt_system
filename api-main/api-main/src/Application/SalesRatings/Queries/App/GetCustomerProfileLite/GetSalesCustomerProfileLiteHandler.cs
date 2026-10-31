using Application.Common.Interfaces;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRatings.Queries.App.GetCustomerProfileLite;

public class GetSalesCustomerProfileLiteHandler : IRequestHandler<GetSalesCustomerProfileLiteQuery, Result<GetSalesCustomerProfileLiteResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSalesCustomerProfileLiteHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetSalesCustomerProfileLiteResponse>> Handle(GetSalesCustomerProfileLiteQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.Id == request.CustomerId)
            .Where(x => x.Orders.Any(o => o.SellerId == request.UserId))
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.PhoneNumber,
                Address = x.Business.Address,
                x.NationalCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            return SalesRatingErrors.CustomerNotFound;

        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.CustomerId == request.CustomerId)
            .Where(x => x.SellerId == request.UserId)
            .Where(x => x.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(x => new
            {
                x.SellAmount,
                Paid = x.InstallmentPayments.Sum(ip => ip.Amount),
                x.SaleDate,
                Payments = x.InstallmentPayments
                    .Select(ip => new SalesCustomerPaymentDto
                    {
                        Id = ip.Id,
                        Amount = ip.Amount,
                        Date = ip.Date
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var totalSales = orders.Sum(x => x.SellAmount);
        var totalPaid = orders.Sum(x => x.Paid);
        var remaining = totalSales - totalPaid;
        var saleDate = orders.Where(x => x.SaleDate is not null).Select(x => x.SaleDate).Min();
        var payments = orders.SelectMany(x => x.Payments).OrderByDescending(x => x.Date).ToList();
        var lastPayment = payments.Count > 0 ? payments[0].Date : (DateOnly?)null;
        var asOf = _dateTimeProvider.Today;
        var score = CustomerRatingCalculator.ScoreFor(totalSales, totalPaid, remaining, saleDate, lastPayment, asOf);
        var level = score is null ? null : CustomerRatingCalculator.LevelName(score.Value);

        return new GetSalesCustomerProfileLiteResponse
        {
            CustomerId = customer.Id,
            CustomerName = customer.FullName,
            Phone = customer.PhoneNumber,
            Address = customer.Address,
            NationalCardNumber = customer.NationalCode,
            RatingLabel = level,
            Score = score,
            IsSettled = remaining <= 0,
            PaymentSummary = new SalesPaymentSummaryDto
            {
                TotalSales = totalSales,
                TotalPaid = totalPaid,
                Remaining = remaining
            },
            Payments = payments
        };
    }
}