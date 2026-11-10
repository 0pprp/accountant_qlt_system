using Application.Common.Interfaces;

namespace Application.SalesRequests.Realtime;

public class SalesRequestChangeNotifier : ISalesRequestChangeNotifier
{
    private readonly IApplicationDbContext _db;

    public SalesRequestChangeNotifier(IApplicationDbContext db)
    {
        _db = db;
    }

    public void Enqueue(SalesRequestChangeMessage message)
        => SalesRequestChangeOutboxWriter.Append(_db, message);
}
