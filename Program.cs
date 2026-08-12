using System.Net;
using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Configuration;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Health;
using DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;
using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAuth;
using DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ChartServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ApplicationConfigurationValidator.ValidateRequiredConfiguration(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.Configure<AdminAuthOptions>(
    builder.Configuration.GetSection(AdminAuthOptions.SectionName));
builder.Services.Configure<RestaurantBrandOptions>(
    builder.Configuration.GetSection(RestaurantBrandOptions.SectionName));
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection(RateLimitingOptions.SectionName));
builder.Services.Configure<ReverseProxyOptions>(
    builder.Configuration.GetSection(ReverseProxyOptions.SectionName));

builder.Services.AddScoped<IAdminCredentialValidator, AdminCredentialValidator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = AuthSchemes.HotCoffeeAdmin;
    options.DefaultSignInScheme = AuthSchemes.HotCoffeeAdmin;
    options.DefaultChallengeScheme = AuthSchemes.HotCoffeeAdmin;
})
.AddCookie(AuthSchemes.HotCoffeeAdmin, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(AdminRoles.Admin));
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBusinessClock, BusinessClock>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();
builder.Services.AddScoped<IPublicMediaUrlResolver, PublicMediaUrlResolver>();
builder.Services.AddScoped<IPublicRestaurantService, PublicRestaurantService>();
builder.Services.AddScoped<IChartService, ChartService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DatabaseReadyHealthCheck>("database", tags: ["ready"]);

builder.Services.AddHotCoffeeRateLimiting();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<AuthenticatedAdminNoStoreFilter>();
});

ConfigureForwardedHeadersIfConfigured(builder);

var app = builder.Build();

if (ShouldUseForwardedHeaders(app.Configuration))
{
    app.UseForwardedHeaders();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Error/404");
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.HasStarted || !string.IsNullOrEmpty(context.Response.ContentType))
    {
        return;
    }

    switch (context.Response.StatusCode)
    {
        case StatusCodes.Status400BadRequest:
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Bad Request");
            break;
        case StatusCodes.Status405MethodNotAllowed:
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Method Not Allowed");
            break;
    }
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = MinimalHealthResponseWriter.WriteAsync
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = MinimalHealthResponseWriter.WriteAsync
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}");

app.Run();

static void ConfigureForwardedHeadersIfConfigured(WebApplicationBuilder builder)
{
    if (!ShouldUseForwardedHeaders(builder.Configuration))
    {
        return;
    }

    var reverseProxy = builder.Configuration
        .GetSection(ReverseProxyOptions.SectionName)
        .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();

        foreach (var proxy in reverseProxy.KnownProxies)
        {
            if (IPAddress.TryParse(proxy, out var address))
            {
                options.KnownProxies.Add(address);
            }
        }

        foreach (var network in reverseProxy.KnownNetworks)
        {
            var parts = network.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2
                && IPAddress.TryParse(parts[0], out var prefix)
                && int.TryParse(parts[1], out var prefixLength))
            {
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength));
            }
        }
    });
}

static bool ShouldUseForwardedHeaders(IConfiguration configuration)
{
    var reverseProxy = configuration
        .GetSection(ReverseProxyOptions.SectionName)
        .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

    if (!reverseProxy.Enabled)
    {
        return false;
    }

    return reverseProxy.KnownProxies.Length > 0 || reverseProxy.KnownNetworks.Length > 0;
}

public partial class Program
{
}
