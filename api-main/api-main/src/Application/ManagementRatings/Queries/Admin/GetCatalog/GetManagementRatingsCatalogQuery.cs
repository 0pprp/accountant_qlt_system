using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetCatalog;

public record GetManagementRatingsCatalogQuery : IRequest<Result<ManagementRatingsCatalogResponse>>
{
    public int BranchId { get; init; }
}
