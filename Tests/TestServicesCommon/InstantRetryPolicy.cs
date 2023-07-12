using System.Net;
using System.Net.Sockets;
using GenericOutbox;
using GenericOutbox.DataAccess.Entities;

namespace TestServicesCommon;

public class InstantRetryStrategy : IRetryStrategy
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

    public TimeSpan? ShouldRetry(Exception ex, OutboxEntity entity)
    {
        var shouldRetry =
            (ex is HttpRequestException httpEx && s_retryStatusCodes.Contains(httpEx.StatusCode))
            || ex is SocketException
            || IsTransientNswagApiException(ex);

        if (shouldRetry)
            return TimeSpan.FromMilliseconds(1);

        return null;
    }

    private bool IsTransientNswagApiException(Exception ex)
    {
        if (ex.GetType().Name != "ApiException")
            return false;

        var statusCodeObj = ex.GetType().GetProperty("StatusCode")?.GetValue(ex);

        if (statusCodeObj == null || statusCodeObj.GetType() != typeof(int))
            return false;

        var statusCode = (int)statusCodeObj;
        return s_retryStatusCodes.Contains((HttpStatusCode?)statusCode);
    }
}