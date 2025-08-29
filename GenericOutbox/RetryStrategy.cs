using System.Net;
using System.Net.Sockets;
using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox;

public class RetryStrategy : IRetryStrategy
{
    private static readonly HttpStatusCode?[] s_retryStatusCodes =
    {
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.FailedDependency,
        HttpStatusCode.GatewayTimeout,
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests
    };

    public ExecutionResult HandleError(Exception ex, OutboxEntity entity)
    {
        var shouldRetry =
            (ex is HttpRequestException httpEx && s_retryStatusCodes.Contains(httpEx.StatusCode))
            || ex is SocketException;

        if (shouldRetry)
            return ExecutionResult.RetryAfter(TimeSpan.FromMinutes(1 << entity.RetriesCount));

        return ExecutionResult.Fail;
    }
}