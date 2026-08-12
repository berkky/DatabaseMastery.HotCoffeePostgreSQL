using System.Threading.RateLimiting;
using DatabaseMastery.HotCoffeePostgreSQL.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddHotCoffeeRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.Headers.CacheControl = "no-store";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter =
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                }

                var message = "Kısa sürede çok fazla istek gönderildi. Lütfen bir süre sonra tekrar deneyin.";
                if (HttpMethods.IsPost(httpContext.Request.Method)
                    && httpContext.Request.Path.StartsWithSegments("/Account/Login"))
                {
                    message = "Kısa sürede çok fazla giriş denemesi yapıldı. Lütfen bir süre sonra tekrar deneyin.";
                }
                else if (HttpMethods.IsPost(httpContext.Request.Method)
                    && httpContext.Request.Path.StartsWithSegments("/Reservation/CreateReservation"))
                {
                    message = "Kısa sürede çok fazla rezervasyon talebi gönderildi. Lütfen bir süre sonra tekrar deneyin.";
                }

                httpContext.Response.ContentType = "text/html; charset=utf-8";
                await httpContext.Response.WriteAsync(
                    $"""
                    <!DOCTYPE html>
                    <html lang="tr">
                    <head><meta charset="utf-8"><title>Çok fazla istek</title></head>
                    <body><main><h1>Çok fazla istek</h1><p>{message}</p></main></body>
                    </html>
                    """,
                    cancellationToken);
            };

            options.AddPolicy(RateLimitPolicies.AdminLoginPost, httpContext =>
            {
                var rateOptions = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimitingOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.AdminLoginPermitLimit,
                        Window = TimeSpan.FromMinutes(rateOptions.AdminLoginWindowMinutes),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicies.PublicReservationPost, httpContext =>
            {
                var rateOptions = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimitingOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.PublicReservationPermitLimit,
                        Window = TimeSpan.FromMinutes(rateOptions.PublicReservationWindowMinutes),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}
