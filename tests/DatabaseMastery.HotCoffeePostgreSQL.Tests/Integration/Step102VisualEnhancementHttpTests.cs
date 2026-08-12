using System.Net;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step102VisualEnhancementHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step102VisualEnhancementHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicRoot_IncludesCinematicHeroMarkers_AndRealDbFacts()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        Assert.Contains("data-hc-hero", html, StringComparison.Ordinal);
        Assert.Contains("hc-hero-cinematic", html, StringComparison.Ordinal);
        Assert.Contains("hc-hero-stage", html, StringComparison.Ordinal);
        Assert.Contains("data-hc-count-to=", html, StringComparison.Ordinal);
        Assert.Contains("Menüyü İncele", html, StringComparison.Ordinal);
        Assert.Contains("Rezervasyon Yap", html, StringComparison.Ordinal);
        Assert.DoesNotContain("dina-html", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dina", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Şefin önerisi", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecoveredMenuImage_Serves200_WithImageContentType()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/images/menu/avocado-tomato.jpg");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("image/", response.Content.Headers.ContentType?.MediaType ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublicCss_IncludesReducedMotionRules()
    {
        var client = _factory.CreateClient();
        var css = await client.GetStringAsync("/css/hotcoffee-public.css");

        Assert.Contains("prefers-reduced-motion: reduce", css, StringComparison.Ordinal);
        Assert.Contains("hc-hero-cinematic", css, StringComparison.Ordinal);
        Assert.Contains("hc-reveal", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicLanding_DoesNotEmitLegacyDinaProductImagePaths()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        Assert.DoesNotContain("/dina-html/images/menu/", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnonymousDashboard_StillRedirectsToLogin()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Dashboard/Index");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
