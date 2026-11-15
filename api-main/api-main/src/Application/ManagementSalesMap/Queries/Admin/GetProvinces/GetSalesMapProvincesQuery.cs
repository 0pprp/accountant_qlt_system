using Application.ManagementSalesMap.Common;

namespace Application.ManagementSalesMap.Queries.Admin.GetProvinces;

public record GetSalesMapProvincesQuery : IRequest<Result<SalesMapProvincesResponse>>;
