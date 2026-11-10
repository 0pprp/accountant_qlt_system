using Application.Common.Interfaces;
using Application.Common.SeedData;
using Application.Customers.Common;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.GetCustomerAuditProfile;

public class GetCustomerAuditProfileHandler
    : IRequestHandler<GetCustomerAuditProfileQuery, Result<GetCustomerAuditProfileResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetCustomerAuditProfileHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<GetCustomerAuditProfileResponse>> Handle(
        GetCustomerAuditProfileQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var canOpen = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadAuditProfile)
                      || _currentUser.HasPermission(SeedPermissions.SalesRequest.MatchRead)
                      || _currentUser.HasPermission(SeedPermissions.Customer.Read);
        if (!canOpen)
            return SalesRequestErrors.ProfileForbidden;

        // الماليات منفصلة عن فتح البروفايل — لا تُستنتج من Customer.Read
        var canFinancial = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadFinancial);
        var canDocuments = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadAuditDocuments)
                           || _currentUser.HasPermission(SeedPermissions.Attachment.Read);

        var customer = await _db.Customers.AsNoTracking()
            .Where(x => x.Id == request.CustomerId && branches.Contains(x.BranchId))
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.MotherName,
                x.NationalCode,
                x.BirthDate,
                x.PhoneNumber,
                Address = x.Business.Address,
                BusinessName = x.Business.Name,
                NearestKnownLocation = x.Business.NearestKnownLocation,
                BranchName = x.Branch!.Name,
                SellAmount = x.Orders.Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved).Sum(o => o.SellAmount),
                PaidAmount = x.Orders.Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .SelectMany(o => o.InstallmentPayments).Sum(ip => ip.Amount),
                SaleDate = x.Orders.Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved && o.SaleDate != null)
                    .Select(o => o.SaleDate).Min(),
                LastPayment = x.Orders.Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .SelectMany(o => o.InstallmentPayments).Select(ip => (DateOnly?)ip.Date).Max()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            return CustomerErrors.CustomerNotFound;

        var sections = ParseSections(request.Sections);
        if (sections.Contains("documents") && !canDocuments)
            return SalesRequestErrors.DocumentsForbidden;

        // shop: جزئي (عنوان) — ليس schema-blocked بالكامل؛ notes/legal: Not implemented
        var available = new List<string> { "overview", "accounts", "sales" };
        if (canDocuments)
            available.Add("documents");
        var schemaRequired = new List<string>();
        var notImplemented = new List<string> { "notes", "legal" };
        var partial = new List<string> { "shop" };

        var remaining = customer.SellAmount - customer.PaidAmount;
        var rating = CustomerRatingCalculator.EvaluateLabel(
            customer.SellAmount, customer.PaidAmount, remaining,
            customer.SaleDate, customer.LastPayment, _clock.Today);

        var response = new GetCustomerAuditProfileResponse
        {
            Header = new CustomerAuditHeaderDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.PhoneNumber,
                BranchName = customer.BranchName,
                StatusLabel = null
            },
            AvailableSections = available,
            SchemaRequiredSections = schemaRequired,
            NotImplementedSections = notImplemented,
            PartialSections = partial,
            Overview = sections.Contains("overview")
                ? new CustomerAuditOverviewDto
                {
                    MotherName = customer.MotherName,
                    NationalCode = customer.NationalCode,
                    BirthDate = customer.BirthDate,
                    Address = customer.Address,
                    BusinessType = customer.BusinessName,
                    RatingLabel = rating,
                    SellTotal = canFinancial ? customer.SellAmount : null,
                    PaidTotal = canFinancial ? customer.PaidAmount : null,
                    Remaining = canFinancial ? remaining : null
                }
                : null,
            Accounts = sections.Contains("accounts")
                ? await LoadAccountsAsync(request.CustomerId, branches, canFinancial, cancellationToken)
                : null,
            Sales = sections.Contains("sales")
                ? await LoadSalesAsync(request.CustomerId, branches, canFinancial, cancellationToken)
                : null,
            Documents = sections.Contains("documents") && canDocuments
                ? await LoadDocumentsAsync(request.CustomerId, branches, cancellationToken)
                : null,
            Shop = null,
            Notes = null
        };

        return response;
    }

    private async Task<CustomerAuditAccountsDto> LoadAccountsAsync(
        int customerId,
        IReadOnlyList<int> branches,
        bool canFinancial,
        CancellationToken ct)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(x => x.CustomerId == customerId
                        && branches.Contains(x.BranchId)
                        && x.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(x => new
            {
                x.Id,
                x.SellAmount,
                Paid = x.InstallmentPayments.Sum(ip => ip.Amount),
                Payments = x.InstallmentPayments
                    .OrderByDescending(ip => ip.Date)
                    .Select(ip => new CustomerAuditPaymentRowDto
                    {
                        Id = ip.Id,
                        Date = ip.Date,
                        Amount = ip.Amount,
                        ReceiptNumber = null,
                        Note = ip.Description
                    }).ToList()
            })
            .ToListAsync(ct);

        return new CustomerAuditAccountsDto
        {
            Rows = orders.Select(o => new CustomerAuditAccountRowDto
            {
                OrderId = o.Id,
                SellAmount = canFinancial ? o.SellAmount : null,
                PaidAmount = canFinancial ? o.Paid : null,
                Remaining = canFinancial ? o.SellAmount - o.Paid : null,
                Payments = canFinancial ? o.Payments : []
            }).ToList()
        };
    }

    private async Task<List<CustomerAuditSaleDto>> LoadSalesAsync(
        int customerId,
        IReadOnlyList<int> branches,
        bool canFinancial,
        CancellationToken ct)
    {
        return await _db.Orders.AsNoTracking()
            .Where(x => x.CustomerId == customerId
                        && branches.Contains(x.BranchId)
                        && x.ApprovalStatus == OrderApprovalStatus.Approved
                        && x.Step == OrderStep.Completed)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CustomerAuditSaleDto
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt!.Value,
                SellerName = x.Seller != null ? x.Seller.FullName : null,
                OrderListName = x.OrderList != null ? x.OrderList.Name : null,
                SellAmount = canFinancial ? x.SellAmount : null,
                PrepaymentAmount = canFinancial ? x.PrepaymentAmount : null,
                Products = x.OrderItems.Select(oi =>
                    oi.ProductId != null ? oi.Product!.Name : (oi.ProductName ?? "")).ToList()
            })
            .ToListAsync(ct);
    }

    private async Task<CustomerAuditDocumentsDto> LoadDocumentsAsync(
        int customerId,
        IReadOnlyList<int> branches,
        CancellationToken ct)
    {
        var customerDocs = await _db.Customers.AsNoTracking()
            .Where(x => x.Id == customerId && branches.Contains(x.BranchId))
            .SelectMany(x => x.Attachments)
            .Select(a => new CustomerAuditDocumentDto
            {
                Id = a.Id,
                OwnerType = "customer",
                OwnerId = customerId,
                OriginalFileName = a.OriginalFileName,
                RelativePath = a.RelativePath,
                Type = a.Type.ToString()
            })
            .ToListAsync(ct);

        var saleDocs = await _db.Orders.AsNoTracking()
            .Where(x => x.CustomerId == customerId && branches.Contains(x.BranchId))
            .SelectMany(x => x.Attachments.Select(a => new CustomerAuditDocumentDto
            {
                Id = a.Id,
                OwnerType = "order",
                OwnerId = x.Id,
                OriginalFileName = a.OriginalFileName,
                RelativePath = a.RelativePath,
                Type = a.Type.ToString()
            }))
            .ToListAsync(ct);

        return new CustomerAuditDocumentsDto
        {
            CustomerDocuments = customerDocs,
            SaleDocuments = saleDocs
        };
    }

    private static HashSet<string> ParseSections(string? sections)
    {
        if (string.IsNullOrWhiteSpace(sections))
            return ["overview"];

        return sections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }
}
