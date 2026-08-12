using System.Net;
using System.Net.Http;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class PublicExperienceHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public PublicExperienceHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_ReturnsPublicHotCoffeeExperience()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("HotCoffee", html, StringComparison.Ordinal);
        Assert.Contains("Menüyü İncele", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Learn about", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dina", html, StringComparison.Ordinal);
        Assert.DoesNotContain("MatchThemes", html, StringComparison.Ordinal);
        Assert.DoesNotContain("sidebar", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Yönetim Paneli", html, StringComparison.Ordinal);
        Assert.Contains("lang=\"tr\"", html, StringComparison.Ordinal);
        Assert.Contains("Product With Reviews", html, StringComparison.Ordinal);
        Assert.Contains("Published Customer", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Hidden Customer", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Hidden comment", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MenuIndex_ReturnsPublicExperience_WithoutAdminShell()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.Contains("HotCoffee", html, StringComparison.Ordinal);
        Assert.Contains("Product With Reviews", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Dina", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yönetim Paneli", html, StringComparison.Ordinal);
        Assert.Contains("hc-public", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateReservation_UsesPublicLayout_AndHasTokenWithoutStatus()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Reservation/CreateReservation");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("hc-public", html, StringComparison.Ordinal);
        Assert.Contains("__RequestVerificationToken", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Status\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ReservationDate", html, StringComparison.Ordinal);
        Assert.Contains("ReservationTime", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yönetim Paneli", html, StringComparison.Ordinal);
        Assert.DoesNotContain("sidebar", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Listeye Dön", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SuccessfulPublicReservation_CreatesPending_AndRedirectsToSuccess()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var formPage = await client.GetAsync("/Reservation/CreateReservation");
        var formHtml = await formPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(formHtml)
            ?? throw new InvalidOperationException("Missing antiforgery token.");

        using var scopeBefore = _factory.Services.CreateScope();
        var dbBefore = scopeBefore.ServiceProvider.GetRequiredService<AppDbContext>();
        var beforeCount = dbBefore.Reservations.Count();

        var response = await client.PostAsync("/Reservation/CreateReservation", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Public Guest",
            ["Phone"] = "+905551112233",
            ["Email"] = "public@example.com",
            ["ReservationDate"] = _factory.BusinessToday.AddDays(3).ToString("yyyy-MM-dd"),
            ["ReservationTime"] = "19:30",
            ["GuestCount"] = "3",
            ["Description"] = "Window seat",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Reservation/Success", response.Headers.Location?.OriginalString);
        Assert.DoesNotContain("ReservationList", response.Headers.Location?.OriginalString ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("Dashboard", response.Headers.Location?.OriginalString ?? string.Empty, StringComparison.Ordinal);

        using var scopeAfter = _factory.Services.CreateScope();
        var dbAfter = scopeAfter.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(beforeCount + 1, dbAfter.Reservations.Count());
        var created = dbAfter.Reservations.Single(r => r.Name == "Public Guest");
        Assert.Equal(ReservationStatus.Pending, created.Status);
    }

    [Fact]
    public async Task ReservationSuccess_IsAnonymousPublicSafe()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Reservation/Success");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Rezervasyon talebiniz alındı", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Onaylandı", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Confirmed", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yönetim Paneli", html, StringComparison.Ordinal);
        Assert.DoesNotContain("@example.com", html, StringComparison.Ordinal);
        Assert.Contains("hc-public", html, StringComparison.Ordinal);
    }
}
