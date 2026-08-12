using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step11ProductionHardeningHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step11ProductionHardeningHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MissingRoute_Returns404_WithoutExceptionDetails()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/this-route-does-not-exist-step11");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sayfa bulunamadı", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublicMenuResponse_IncludesSecurityHeaders()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Menu/Index");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHeaderEquals(response, "X-Content-Type-Options", "nosniff");
        AssertHeaderEquals(response, "Referrer-Policy", "strict-origin-when-cross-origin");
        AssertHeaderEquals(response, "X-Frame-Options", "DENY");
        AssertHeaderContains(response, "Permissions-Policy", "geolocation=()");
        AssertHeaderContains(response, "Content-Security-Policy", "frame-ancestors 'none'");
    }

    [Fact]
    public async Task AuthenticatedDashboardResponse_IncludesSecurityHeaders_AndNoStore()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Dashboard/Index");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHeaderEquals(response, "X-Content-Type-Options", "nosniff");
        AssertHeaderEquals(response, "X-Frame-Options", "DENY");
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthyWithoutDatabaseDetails()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"Healthy\"", body.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Connection", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("postgres", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthReady_ReturnsHealthyInIsolatedTestHost()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"Healthy\"", body.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginPost_RateLimited_Returns429WithRetryAfter()
    {
        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<Configuration.RateLimitingOptions>(options =>
                {
                    options.AdminLoginPermitLimit = 2;
                    options.AdminLoginWindowMinutes = 5;
                });
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)
            ?? throw new InvalidOperationException("Antiforgery token missing on login page.");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var attemptResponse = await client.PostAsync(
                "/Account/Login",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Username"] = "wrong-user",
                    ["Password"] = "wrong-password",
                    ["__RequestVerificationToken"] = token
                }));

            Assert.NotEqual(HttpStatusCode.TooManyRequests, attemptResponse.StatusCode);
        }

        var limitedResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = "wrong-user",
                ["Password"] = "wrong-password",
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        Assert.True(limitedResponse.Headers.Contains("Retry-After"));
        var limitedHtml = await limitedResponse.Content.ReadAsStringAsync();
        Assert.Contains("Çok fazla istek", limitedHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReservationPost_RateLimited_Returns429WithoutDatabaseWrite()
    {
        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<Configuration.RateLimitingOptions>(options =>
                {
                    options.PublicReservationPermitLimit = 2;
                    options.PublicReservationWindowMinutes = 10;
                });
            });
        });

        _factory.EnsureSeeded();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context.AppDbContext>();
        var reservationCountBefore = db.Reservations.Count();
        var businessToday = _factory.BusinessToday;

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var formPage = await client.GetAsync("/Reservation/CreateReservation");
        var html = await formPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)
            ?? throw new InvalidOperationException("Antiforgery token missing on reservation page.");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var attemptResponse = await PostReservationAsync(client, businessToday, token);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, attemptResponse.StatusCode);
        }

        var limitedResponse = await PostReservationAsync(client, businessToday, token);
        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<Context.AppDbContext>();
        Assert.Equal(reservationCountBefore + 2, verifyDb.Reservations.Count());
    }

    [Fact]
    public async Task HealthReady_ReturnsUnhealthyWhenDatabaseCheckFails()
    {
        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    var readyChecks = options.Registrations
                        .Where(registration => registration.Tags.Contains("ready"))
                        .ToList();

                    foreach (var registration in readyChecks)
                    {
                        options.Registrations.Remove(registration);
                    }

                    options.Registrations.Add(new HealthCheckRegistration(
                        "database",
                        _ => new UnhealthyHealthCheck(),
                        HealthStatus.Unhealthy,
                        ["ready"]));
                });
            });
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("\"status\":\"Unhealthy\"", body.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpResponseMessage> PostReservationAsync(
        HttpClient client,
        DateOnly businessToday,
        string token)
    {
        return await client.PostAsync(
            "/Reservation/CreateReservation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = "Rate Limit Guest",
                ["Phone"] = "+905551112233",
                ["Email"] = "guest@example.com",
                ["GuestCount"] = "2",
                ["ReservationDate"] = businessToday.AddDays(1).ToString("yyyy-MM-dd"),
                ["ReservationTime"] = "19:00",
                ["__RequestVerificationToken"] = token
            }));
    }

    private static void AssertHeaderEquals(HttpResponseMessage response, string name, string expected)
    {
        Assert.True(response.Headers.TryGetValues(name, out var values), $"Missing header {name}.");
        Assert.Equal(expected, values.Single());
    }

    private static void AssertHeaderContains(HttpResponseMessage response, string name, string expectedPart)
    {
        Assert.True(response.Headers.TryGetValues(name, out var values), $"Missing header {name}.");
        Assert.Contains(expectedPart, values!.Single(), StringComparison.Ordinal);
    }

    private sealed class UnhealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}
