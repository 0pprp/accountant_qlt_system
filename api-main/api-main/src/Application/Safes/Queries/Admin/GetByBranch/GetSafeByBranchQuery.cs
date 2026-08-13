namespace Application.Safes.Queries.Admin.GetByBranch;

public record GetSafeByBranchQuery(int? BranchId) : IRequest<Result<GetSafeByBranchResponse>>;