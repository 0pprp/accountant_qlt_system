namespace Application.SalesRequests.Queries.App.TransferPeers;

/// <summary>عقد جديد: زملاء مؤهلون لنقل الاسم ضمن فرع الطلب المملوك للجلسة.</summary>
public sealed record GetTransferPeersQuery(int RequestId, string? Search = null)
    : IRequest<Result<List<TransferPeerItem>>>;

public sealed record TransferPeerItem
{
    public int Id { get; init; }
    public string FullName { get; init; } = "";
    public string? Username { get; init; }
}
