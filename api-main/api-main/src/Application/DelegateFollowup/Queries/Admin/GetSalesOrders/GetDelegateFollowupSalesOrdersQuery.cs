using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetSalesOrders;

public record GetDelegateFollowupSalesOrdersQuery(int ListId, int BranchId) : IRequest<Result<DelegateSalesOrdersDto>>;
