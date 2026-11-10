using Application.FollowerWork.Common;

namespace Application.OrderLists.Commands.Admin.SetFollowerCollectionEnabled;

public sealed class SetFollowerCollectionEnabledCommand
    : IRequest<Result<SetFollowerCollectionEnabledResultDto>>
{
    public int ListId { get; init; }
    public bool Enabled { get; init; }
    public int? ExpectedVersion { get; init; }
}

public sealed class SetFollowerCollectionEnabledBody
{
    public bool Enabled { get; set; }
    public int? ExpectedVersion { get; set; }
}
