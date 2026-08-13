namespace Application.Common.Interfaces;

public interface IHttpClientWrapper
{
    Task<Result<string>> ExecuteAsync(HttpRequestMessage request, string callerMemberName = "", CancellationToken cancellationToken = default);
}