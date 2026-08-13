namespace Application.Branches.Queries.Admin.GetBranchWarehouses;

public record GetBranchWarehousesQuery(int BranchId) : IRequest<Result<List<GetBranchWarehousesResponse>>>;