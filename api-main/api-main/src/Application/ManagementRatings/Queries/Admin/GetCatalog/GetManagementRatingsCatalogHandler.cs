using Application.Common.Interfaces;
using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetCatalog;

public class GetManagementRatingsCatalogHandler
    : IRequestHandler<GetManagementRatingsCatalogQuery, Result<ManagementRatingsCatalogResponse>>
{
    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetManagementRatingsCatalogHandler(ManagementRatingsDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementRatingsCatalogResponse>> Handle(
        GetManagementRatingsCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var err = ManagementRatingsAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (err is not null)
            return err.Value;

        return await _data.GetCatalogAsync(request.BranchId, cancellationToken);
    }
}
