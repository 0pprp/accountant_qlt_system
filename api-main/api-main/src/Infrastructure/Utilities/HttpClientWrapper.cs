using System.Runtime.CompilerServices;
using Application.Common;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Utilities;

public class HttpClientWrapper : IHttpClientWrapper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientWrapper> _logger;
    private const string FailureLogTemplate = "[HttpRequestFailure] {MethodName} :: Sending request failed.  - {Request} - {RequestContent} - {Response}";
    private const string SuccessLogTemplate = "[HttpRequestSuccess] {MethodName} :: Sending request succeeded.  - {Request} - {RequestContent} - {Response}";

    public HttpClientWrapper(HttpClient httpClient, ILogger<HttpClientWrapper> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<string>> ExecuteAsync(HttpRequestMessage request, [CallerMemberName] string callerMemberName = "", CancellationToken cancellationToken = default)
    {
        var responseContent = string.Empty;
        var requestContent = request.Content is not null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation(SuccessLogTemplate, callerMemberName, request.ToString(), requestContent, responseContent);

            return responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailureLogTemplate, callerMemberName, request.ToString(), requestContent, responseContent);
            return new Error("Failed to send http request", "Http_Request_Failed");
        }
    }
}