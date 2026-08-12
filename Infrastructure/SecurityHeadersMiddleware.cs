namespace DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;

public sealed class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "frame-ancestors 'none'; object-src 'none'; base-uri 'self'; form-action 'self'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-Frame-Options"] = "DENY";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        headers["Content-Security-Policy"] = ContentSecurityPolicy;

        await _next(context);
    }
}
