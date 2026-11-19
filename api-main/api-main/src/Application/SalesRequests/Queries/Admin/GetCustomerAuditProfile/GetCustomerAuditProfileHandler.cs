using Application.Common.Interfaces;
using Application.Common.SeedData;
using Application.Customers.Common;
using Application.SalesRequests.Common;
using Domain.Entities.CustomerAggregate;
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
                GpsLatitude = x.Business.GpsLatitude,
                GpsLongitude = x.Business.GpsLongitude,
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
        var available = new List<string> { "overview", "accounts", "sales", "ratings" };
        if (canDocuments)
            available.Add("documents");
        var schemaRequired = new List<string>();
        var notImplemented = new List<string> { "notes", "legal" };
        var partial = new List<string> { "shop" };

        var unallocatedPaid = (await Application.Common.Balances.CustomerBalanceQuery.UnallocatedByCustomerAsync(
            _db, [request.CustomerId], null, cancellationToken)).GetValueOrDefault(request.CustomerId);
        var paidTotal = customer.PaidAmount + unallocatedPaid;
        var remaining = Application.Common.Balances.CustomerBalanceRules.RoundToUnit(customer.SellAmount - paidTotal);
        var rating = CustomerRatingCalculator.EvaluateLabel(
            customer.SellAmount, paidTotal, customer.SellAmount - paidTotal,
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
                    PaidTotal = canFinancial ? paidTotal : null,
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
            RatingsAndLocation = sections.Contains("ratings")
                ? await LoadRatingsAndLocationAsync(
                    request.CustomerId,
                    customer.GpsLatitude,
                    customer.GpsLongitude,
                    cancellationToken)
                : null,
            Shop = sections.Contains("shop") || sections.Contains("ratings")
                ? await LoadShopAsync(request.CustomerId, cancellationToken)
                : null,
            Notes = null
        };

        return response;
    }

    private async Task<CustomerAuditRatingsAndLocationDto> LoadRatingsAndLocationAsync(
        int customerId,
        double? gpsLatitude,
        double? gpsLongitude,
        CancellationToken ct)
    {
        var ratings = await _db.CustomerStaffRatings.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(ct);

        CustomerAuditStaffRatingDto? Map(string role, string title)
        {
            var row = ratings.FirstOrDefault(r =>
                string.Equals(r.Role, role, StringComparison.OrdinalIgnoreCase));
            if (row is null)
                return new CustomerAuditStaffRatingDto { Role = role, Title = title };
            return new CustomerAuditStaffRatingDto
            {
                Role = role,
                Title = title,
                Score = row.Score,
                Note = row.Note,
                RatedByName = row.RatedByName,
                RatedAtUtc = row.RatedAtUtc,
                OrderId = row.OrderId,
                ShopStockEstimatedValue = row.ShopStockEstimatedValue,
                EstimatedDailyRevenue = row.EstimatedDailyRevenue,
                ContextAmount = row.ContextAmount,
                Occasion = row.Occasion,
                OccasionLabel = row.Occasion switch
                {
                    CustomerStaffRating.OccasionAuthentication => "عند المصادقة على البيع",
                    CustomerStaffRating.OccasionSale => "عند البيع",
                    CustomerStaffRating.OccasionFirstCollection => "عند أول تسديد",
                    _ => null
                },
                IsWrongSale = row.IsWrongSale
            };
        }

        var latestOrderLoc = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId && o.Location != null)
            .OrderByDescending(o => o.Id)
            .Select(o => o.Location)
            .FirstOrDefaultAsync(ct);

        var hasGps = gpsLatitude is double lat
                     && gpsLongitude is double lng
                     && !(Math.Abs(lat) < 1e-9 && Math.Abs(lng) < 1e-9);

        var snapshots = await LoadRatingSnapshotsAsync(customerId, ct);

        var survey = await _db.CustomerShopProfiles.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .Select(x => new CustomerAuditShopSurveyDto
            {
                Title = "كشف المحل — موظف المبيعات",
                ShopName = x.ShopName,
                BusinessType = x.BusinessType,
                StockEstimatedValue = x.StockEstimatedValue,
                EstimatedDailyRevenue = x.EstimatedDailyRevenue,
                ShopLength = x.ShopLength,
                ShopWidth = x.ShopWidth,
                ShopArea = x.ShopArea,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(ct);

        var locations = await _db.CustomerLocations.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Source)
            .Select(x => new CustomerAuditLocationDto
            {
                Source = x.Source.ToString(),
                SourceLabel = x.Source == CustomerLocationSource.Captured ? "موقع ملتقط ميدانياً"
                    : x.Source == CustomerLocationSource.Estimated ? "موقع مُقدَّر من نقاط المندوب"
                    : x.Source == CustomerLocationSource.ShopProfile ? "موقع من بروفايل المحل"
                    : "موقع من الملف القانوني",
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AccuracyMeters = x.AccuracyMeters,
                Confidence = x.Confidence,
                CapturedAtUtc = x.CapturedAtUtc
            })
            .ToListAsync(ct);

        var photos = await _db.CustomerShopPhotos.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .Select(x => new CustomerAuditPhotoDto
            {
                Id = x.Id,
                ContentType = x.ContentType,
                CapturedAtUtc = x.CapturedAtUtc
            })
            .ToListAsync(ct);

        if (survey is not null)
        {
            var surveyLocation = locations.FirstOrDefault(x =>
                x.Source == nameof(CustomerLocationSource.ShopProfile));
            survey = survey with
            {
                Latitude = surveyLocation?.Latitude,
                Longitude = surveyLocation?.Longitude,
                PhotoId = photos.FirstOrDefault()?.Id
            };
        }

        return new CustomerAuditRatingsAndLocationDto
        {
            Snapshots = snapshots,
            Locations = locations,
            Photos = photos,
            DelegateRating = Map(CustomerStaffRating.RoleDelegate, "تقييم المندوب"),
            CommitteeFieldRating = Map(
                CustomerStaffRating.RoleCommitteeField,
                "تقييم مسؤول اللجنة الميدانية"),
            ShopSurvey = survey,
            GpsLatitude = hasGps ? gpsLatitude : null,
            GpsLongitude = hasGps ? gpsLongitude : null,
            HasGps = hasGps,
            LatestOrderLatitude = latestOrderLoc?.Latitude,
            LatestOrderLongitude = latestOrderLoc?.Longitude
        };
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

        var imported = await _db.CustomerDocuments.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Id)
            .Select(x => new CustomerAuditDocumentDto
            {
                Id = x.Id,
                OwnerType = x.OrderId != null ? "order" : "customer",
                OwnerId = x.OrderId ?? customerId,
                OriginalFileName = x.FileName ?? x.FileKey,
                RelativePath = x.FileKey,
                Type = x.DocumentType
            })
            .ToListAsync(ct);

        customerDocs.AddRange(imported.Where(x => x.OwnerType == "customer"));
        saleDocs.AddRange(imported.Where(x => x.OwnerType == "order"));

        return new CustomerAuditDocumentsDto
        {
            CustomerDocuments = customerDocs,
            SaleDocuments = saleDocs
        };
    }

    private async Task<CustomerAuditShopDto?> LoadShopAsync(int customerId, CancellationToken ct)
    {
        var profile = await _db.CustomerShopProfiles.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.ShopName, x.BusinessType, x.StockEstimatedValue,
                x.EstimatedDailyRevenue, x.ShopLength, x.ShopWidth, x.ShopArea, x.ShopImageKey
            })
            .FirstOrDefaultAsync(ct);

        var location = await _db.CustomerLocations.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Source)
            .Select(x => new { x.Latitude, x.Longitude, x.Source })
            .FirstOrDefaultAsync(ct);

        var photoId = await _db.CustomerShopPhotos.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var address = await _db.Customers.AsNoTracking()
            .Where(x => x.Id == customerId)
            .Select(x => x.Business.Address)
            .FirstOrDefaultAsync(ct);

        if (profile is null && location is null && photoId is null)
            return null;

        return new CustomerAuditShopDto
        {
            Address = address,
            Location = location is null
                ? null
                : $"{location.Latitude:0.######},{location.Longitude:0.######}",
            ImplementationStatus = "ready",
            ShopName = profile?.ShopName,
            BusinessType = profile?.BusinessType,
            StockEstimatedValue = profile?.StockEstimatedValue,
            EstimatedDailyRevenue = profile?.EstimatedDailyRevenue,
            ShopLength = profile?.ShopLength,
            ShopWidth = profile?.ShopWidth,
            ShopArea = profile?.ShopArea,
            ShopImageKey = profile?.ShopImageKey,
            ShopPhotoId = photoId,
            LocationSource = location?.Source.ToString()
        };
    }

    private async Task<List<CustomerAuditRatingSnapshotDto>> LoadRatingSnapshotsAsync(
        int customerId, CancellationToken ct)
    {
        return await _db.CustomerRatingSnapshots.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CaptureDate)
            .Take(60)
            .Select(x => new CustomerAuditRatingSnapshotDto
            {
                Origin = x.Origin,
                RatingLabel = x.RatingLabel,
                Score = x.Score,
                CaptureDate = x.CaptureDate
            })
            .ToListAsync(ct);
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
