using System.Net;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step101VisualQaHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step101VisualQaHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateReservation_Get_DoesNotExposeMinValueDefaults()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Reservation/CreateReservation");

        Assert.DoesNotContain("0001-01-01", html, StringComparison.Ordinal);
        Assert.DoesNotContain("01.01.0001", html, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"00:00\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("type=\"date\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("type=\"time\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateReservation_PostWithoutDateOrTime_ReturnsValidationErrors()
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
            ["Name"] = "Missing Date Guest",
            ["Phone"] = "+905551112233",
            ["Email"] = "missing@example.com",
            ["GuestCount"] = "2",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Rezervasyon tarihi zorunludur", html, StringComparison.Ordinal);
        Assert.Contains("Rezervasyon saati zorunludur", html, StringComparison.Ordinal);

        using var scopeAfter = _factory.Services.CreateScope();
        var dbAfter = scopeAfter.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(beforeCount, dbAfter.Reservations.Count());
    }

    [Fact]
    public async Task Dashboard_TodayReservationsWidget_UsesCompactColumns()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/Dashboard/Index");

        Assert.Contains("Bugünkü Rezervasyonlar", html, StringComparison.Ordinal);
        Assert.Contains("dash-res-table-compact", html, StringComparison.Ordinal);
        Assert.Contains(">Misafir<", html, StringComparison.Ordinal);
        Assert.Contains(">Saat<", html, StringComparison.Ordinal);
        Assert.Contains(">Kişi<", html, StringComparison.Ordinal);
        Assert.Contains(">Durum<", html, StringComparison.Ordinal);
        Assert.DoesNotContain(">Telefon<", html, StringComparison.Ordinal);
        Assert.DoesNotContain(">Not<", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicLanding_ContinuesServingActiveProducts_WithoutDinaShell()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        Assert.Contains("Product With Reviews", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-fallback-mark", html, StringComparison.Ordinal);
        Assert.DoesNotContain("dina-html", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dina", html, StringComparison.Ordinal);
    }
}
