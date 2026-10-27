using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Customers.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.Admin.GetById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCustomerByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetCustomerByIdResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var asOf = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        var row = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.MotherName,
                x.NationalCode,
                x.BirthDate,
                x.PhoneNumber,
                x.WhatsAppPhoneNumber,
                x.Business,
                BranchId = x.BranchId,
                BranchName = x.Branch!.Name,
                Attachments = x.Attachments.Select(a => new GetAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    RelativePath = a.RelativePath,
                    FileSizeInByte = a.FileSizeInByte,
                    Type = a.Type
                }).ToList(),
                SellAmount = x.Orders.Sum(o => o.SellAmount),
                PaidAmount = x.Orders.SelectMany(o => o.InstallmentPayments).Sum(ip => ip.Amount),
                SaleDate = x.Orders.Where(o => o.SaleDate != null).Select(o => o.SaleDate).Min(),
                EarliestOrderCreated = x.Orders.Where(o => o.CreatedAt != null)
                    .Select(o => (DateTimeOffset?)o.CreatedAt).Min(),
                LastInstallmentPaymentDate = x.Orders.SelectMany(o => o.InstallmentPayments).Any()
                    ? x.Orders.SelectMany(o => o.InstallmentPayments).Max(ip => ip.Date)
                    : (DateOnly?)null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return CustomerErrors.CustomerNotFound;

        var saleDate = row.SaleDate
                       ?? (row.EarliestOrderCreated is { } created
                           ? DateOnly.FromDateTime(created.DateTime)
                           : (DateOnly?)null);
        var remaining = row.SellAmount - row.PaidAmount;

        return new GetCustomerByIdResponse
        {
            Id = row.Id,
            FullName = row.FullName,
            MotherName = row.MotherName,
            NationalCode = row.NationalCode,
            BirthDate = row.BirthDate,
            PhoneNumber = row.PhoneNumber,
            WhatsAppPhoneNumber = row.WhatsAppPhoneNumber,
            Rating = CustomerRatingCalculator.EvaluateLabel(
                row.SellAmount,
                row.PaidAmount,
                remaining,
                saleDate,
                row.LastInstallmentPaymentDate,
                asOf),
            Business = row.Business,
            Branch = new GetBranchDto
            {
                Id = row.BranchId,
                Name = row.BranchName
            },
            Attachments = row.Attachments
        };
    }
}
