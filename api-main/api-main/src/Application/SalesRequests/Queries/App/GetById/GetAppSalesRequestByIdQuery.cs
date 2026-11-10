using Application.SalesRequests.Common;

namespace Application.SalesRequests.Queries.App.GetById;

public record GetAppSalesRequestByIdQuery(int RequestId)
    : IRequest<Result<SalesRequestAppItemDto>>;
