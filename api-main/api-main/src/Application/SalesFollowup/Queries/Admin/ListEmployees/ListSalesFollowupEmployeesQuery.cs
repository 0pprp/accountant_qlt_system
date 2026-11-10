using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Queries.Admin.ListEmployees;

public record ListSalesFollowupEmployeesQuery : IRequest<Result<ListSalesFollowupEmployeesResponse>>
{
    /// <summary>null أو غير مُرسل = كل الفروع المصرّح بها.</summary>
    public int? BranchId { get; init; }
}

public class ListSalesFollowupEmployeesResponse
{
    public List<SalesFollowupEmployeeCardDto> Items { get; set; } = [];
    public string Marker { get; set; } = SalesFollowupRules.DeployMarker;
}
