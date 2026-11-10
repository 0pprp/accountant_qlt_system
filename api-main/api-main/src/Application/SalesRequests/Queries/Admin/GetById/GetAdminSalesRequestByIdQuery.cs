using Application.SalesRequests.Queries.Admin.List;

namespace Application.SalesRequests.Queries.Admin.GetById;

public record GetAdminSalesRequestByIdQuery(int Id)
    : IRequest<Result<AdminSalesRequestListItem>>;
