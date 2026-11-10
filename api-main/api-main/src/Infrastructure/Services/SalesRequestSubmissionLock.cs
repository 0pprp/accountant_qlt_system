using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SalesRequestSubmissionLock(IApplicationDbContext db) : ISalesRequestSubmissionLock
{
    public Task AcquireAsync(int userId, Guid submissionKey, CancellationToken cancellationToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"sales-request:{userId}:{submissionKey:D}"));
        var lockId = BinaryPrimitives.ReadInt64BigEndian(hash);
        return db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockId})", cancellationToken);
    }
}
