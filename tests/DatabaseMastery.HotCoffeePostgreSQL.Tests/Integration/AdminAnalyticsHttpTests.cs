using System.Net;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class AdminAnalyticsHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public AdminAnalyticsHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Dashboard_Authenticated_Returns200_WithoutFakeClaims()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Dashboard/Index");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Bugünkü Sipariş", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Masa Doluluk", html, StringComparison.Ordinal);
        Assert.DoesNotContain("₺ 28.450", html, StringComparison.Ordinal);
        Assert.DoesNotContain("21/30", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ahmet Yılmaz", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Kampanya Oluştur", html, StringComparison.Ordinal);
        Assert.Contains("Toplam Rezervasyon", html, StringComparison.Ordinal);
        Assert.Contains("Ortalama Puan", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Statistics_Authenticated_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Statistics/Index");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Tekrar Gelen", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yeni Müşteri", html, StringComparison.Ordinal);
        Assert.Contains("Toplam Rezervasyon", html, StringComparison.Ordinal);
        Assert.Contains("Operasyon Metrikleri", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminLayout_Authenticated_RedirectsToDashboard()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/AdminLayout/Index");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Dashboard", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AdminLayout_Anonymous_ChallengesLogin()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/AdminLayout/Index");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
