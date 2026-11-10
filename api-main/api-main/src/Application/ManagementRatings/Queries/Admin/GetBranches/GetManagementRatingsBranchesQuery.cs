using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetBranches;

public record GetManagementRatingsBranchesQuery : IRequest<Result<ManagementRatingsBranchesResponse>>;
