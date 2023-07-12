using System.Net;
using Microsoft.AspNetCore.Connections.Features;

public class TransientErrorsMiddleware : IMiddleware
{
    private static readonly HttpStatusCode[] s_retryStatusCodes =
    {
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.FailedDependency,
        HttpStatusCode.GatewayTimeout,
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var rnd = Random.Shared.NextDouble();

        if (rnd > 0.35)
        {
            await next(context);
            return;
        }

        var errorId = Random.Shared.Next(s_retryStatusCodes.Length + 1);

        if (s_retryStatusCodes.Length > errorId)
        {
            context.Response.StatusCode = (int)s_retryStatusCodes[errorId];
            return;
        }
        //drop connection

        var connectionLifetime = context.Features.Get<IConnectionLifetimeFeature>();
        if (connectionLifetime != null) //it apparently is null if you are using WebApplicationFactory in tests
        {
            connectionLifetime.Abort();
            return;
        }

        await next(context);
    }
}