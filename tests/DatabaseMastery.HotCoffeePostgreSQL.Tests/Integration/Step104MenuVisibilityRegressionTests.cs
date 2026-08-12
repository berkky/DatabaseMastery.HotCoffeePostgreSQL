using System.Text.RegularExpressions;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step104MenuVisibilityRegressionTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step104MenuVisibilityRegressionTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicMenu_ContainsRenderableProductCatalog_InHtml()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.Contains("Sakin bir masa, özenli bir menü", html, StringComparison.Ordinal);
        Assert.Contains("id=\"menu-heading\"", html, StringComparison.Ordinal);
        Assert.Contains("hc-category-nav", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-card", html, StringComparison.Ordinal);
        Assert.Contains("Product With Reviews", html, StringComparison.Ordinal);
        Assert.Contains("Product Without Reviews", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicRoot_RedirectsToMenu_WithSameMenuMarkers()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = true });
        var html = await client.GetStringAsync("/");

        Assert.Contains("id=\"menu\"", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-card", html, StringComparison.Ordinal);
        Assert.Contains("Product With Reviews", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicCss_RevealUsesMotionReadyGuard_NotUnconditionalJsHide()
    {
        var client = _factory.CreateClient();
        var css = await client.GetStringAsync("/css/hotcoffee-public.css");

        Assert.Contains(".hc-motion-ready .hc-reveal:not(.is-visible)", css, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"\.js\s+\.hc-reveal\s*\{[^}]*opacity\s*:\s*0", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            css);
    }

    [Fact]
    public async Task PublicJs_EnablesMotionReady_AfterRevealInitialization()
    {
        var client = _factory.CreateClient();
        var js = await client.GetStringAsync("/js/hotcoffee-public.js");

        Assert.Contains("hc-motion-ready", js, StringComparison.Ordinal);
        Assert.Contains("markAllRevealsVisible", js, StringComparison.Ordinal);
        Assert.Contains("IntersectionObserver", js, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicMenu_ProductCardsAreNotRevealGated()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.DoesNotMatch(
            new Regex(@"hc-product-card\s+hc-reveal", RegexOptions.IgnoreCase),
            html);
        Assert.DoesNotMatch(
            new Regex("id=\"menu\"[^>]*hc-reveal", RegexOptions.IgnoreCase),
            html);
    }

    [Fact]
    public async Task PublicMenu_PreservesVerifiedPhotoMappings_AndFallbacks()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.DoesNotContain("/images/menu/products/", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-fallback-mark", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/dina-html/", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Menu IMG", html, StringComparison.Ordinal);
    }
}
