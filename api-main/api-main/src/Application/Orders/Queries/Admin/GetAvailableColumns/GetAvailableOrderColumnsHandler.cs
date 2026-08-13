using Application.Orders.Common;

namespace Application.Orders.Queries.Admin.GetAvailableColumns;

public class GetAvailableOrderColumnsHandler : IRequestHandler<GetAvailableOrderColumnsQuery, Result<List<GetAvailableOrderColumnsResponse>>>
{
    public Task<Result<List<GetAvailableOrderColumnsResponse>>> Handle(GetAvailableOrderColumnsQuery request,
        CancellationToken cancellationToken)
    {
        var columns = OrderColumns.All
            .Select(x => new GetAvailableOrderColumnsResponse
            {
                Key = x.Key,
                DisplayName = x.DisplayName,
                DataType = x.DataType
            })
            .ToList();

        return Task.FromResult<Result<List<GetAvailableOrderColumnsResponse>>>(columns);
    }
}

