namespace Application.Common.Interfaces;

public interface ISalesRequestSubmissionLock
{
    Task AcquireAsync(int userId, Guid submissionKey, CancellationToken cancellationToken);
}
