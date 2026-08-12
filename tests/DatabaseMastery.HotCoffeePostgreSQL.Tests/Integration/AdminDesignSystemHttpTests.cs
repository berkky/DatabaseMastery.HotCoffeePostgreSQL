using System.Net;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class AdminDesignSystemHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public AdminDesignSystemHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_UsesHotCoffeeAuthShell_WithoutAdminSidebar()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Account/Login");

        Assert.Contains("HotCoffee Admin", html, StringComparison.Ordinal);
        Assert.Contains("hc-login", html, StringComparison.Ordinal);
        Assert.Contains("hotcoffee-login.css", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-hc-shell=\"admin\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"sidebar\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Lezzet Bahçesi", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dashboard_Authenticated_UsesAdminShellAndSharedAssets()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var html = await client.GetStringAsync("/Dashboard/Index");

        Assert.Contains("data-hc-shell=\"admin\"", html, StringComparison.Ordinal);
        Assert.Contains("hc-admin", html, StringComparison.Ordinal);
        Assert.Contains("HotCoffee", html, StringComparison.Ordinal);
        Assert.Contains("hotcoffee-admin.css", html, StringComparison.Ordinal);
        Assert.Contains("hotcoffee-admin-pages.css", html, StringComparison.Ordinal);
        Assert.Contains("hotcoffee-admin-dashboard.css", html, StringComparison.Ordinal);
        Assert.Contains("aria-expanded", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Siparişler", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Müşteriler", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Kampanya", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ahmet Yılmaz", html, StringComparison.Ordinal);
        Assert.DoesNotContain("₺ 28.450", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicLanding_HasMobileNavSemantics()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        Assert.Contains("hc-nav-toggle", html, StringComparison.Ordinal);
        Assert.Contains("aria-expanded=\"false\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"hcPrimaryNav\"", html, StringComparison.Ordinal);
        Assert.Contains("hc-public", html, StringComparison.Ordinal);
        Assert.Contains("hotcoffee-tokens.css", html, StringComparison.Ordinal);
    }
}
