using Application.Users.Common;

namespace Application.Users.Queries.Admin.GetAvailableColumns;

public class GetAvailableUserColumnsHandler : IRequestHandler<GetAvailableUserColumnsQuery, Result<List<GetAvailableUserColumnsResponse>>>
{
    public Task<Result<List<GetAvailableUserColumnsResponse>>> Handle(GetAvailableUserColumnsQuery request,
        CancellationToken cancellationToken)
    {
        var columns = UserColumns.All
            .Select(x => new GetAvailableUserColumnsResponse
            {
                Key = x.Key,
                DisplayName = x.DisplayName,
                DataType = x.DataType
            })
            .ToList();

        return Task.FromResult<Result<List<GetAvailableUserColumnsResponse>>>(columns);
    }
}
