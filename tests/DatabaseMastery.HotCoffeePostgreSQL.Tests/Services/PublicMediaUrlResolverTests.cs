using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Services;

public class PublicMediaUrlResolverTests
{
    [Fact]
    public void ResolveProductImageUrl_RejectsTemplatePlaceholder_EvenWhenLegacyPathExists()
    {
        var webRoot = FindWebRootPath();
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(webRoot));
        var target = Path.Combine(webRoot, "images", "menu", "avocado-tomato.jpg");
        Assert.True(File.Exists(target), $"Expected recovered asset at {target}");
        Assert.True(ProductMediaCatalog.IsTemplatePlaceholderAsset(target));

        var resolved = resolver.ResolveProductImageUrl(0, "/dina-html/images/menu/avocado-tomato.jpg");

        Assert.Null(resolved);
    }

    [Fact]
    public void ResolveProductImageUrl_ReturnsNull_WhenAssetMissing()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        var resolved = resolver.ResolveProductImageUrl(0, "/images/menu/does-not-exist.jpg");

        Assert.Null(resolved);
    }

    [Fact]
    public void ResolveProductImageUrl_RejectsFilenameMatch_WhenOwnedAssetIsPlaceholder()
    {
        var webRoot = FindWebRootPath();
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(webRoot));

        var resolved = resolver.ResolveProductImageUrl(0, "/legacy/template/menu/roast-chicken.jpg");

        Assert.Null(resolved);
    }

    [Fact]
    public void ResolveProductImageUrl_BlocksUnsafeSchemes()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        Assert.Null(resolver.ResolveProductImageUrl(0, "javascript:alert(1)"));
        Assert.Null(resolver.ResolveProductImageUrl(0, "data:text/html,test"));
    }

    [Fact]
    public void ResolveProductImageUrl_BlocksExternalUrls()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        Assert.Null(resolver.ResolveProductImageUrl(0, "https://example.com/menu.jpg"));
        Assert.Null(resolver.ResolveProductImageUrl(0, "http://example.com/menu.jpg"));
    }

    [Fact]
    public void ResolveHeroStageImageUrl_ReturnsOwnedHeroAsset_WhenPresent()
    {
        var webRoot = FindWebRootPath();
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(webRoot));

        var resolved = resolver.ResolveHeroStageImageUrl();

        Assert.Equal(MenuMediaCatalog.HeroStageImagePath, resolved);
        Assert.False(ProductMediaCatalog.IsTemplatePlaceholderAsset(
            Path.Combine(webRoot, "images", "menu", "menu-hero-3d.jpg")));
    }

    [Fact]
    public void ResolveProductImageUrl_EmptyCatalog_ReturnsNull_ForFormerMappedIds()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        Assert.Empty(ProductMediaCatalog.VerifiedMappings);
        Assert.Null(resolver.ResolveProductImageUrl(1, "/images/products/bruschetta.jpg"));
        Assert.Null(resolver.ResolveProductImageUrl(4, "/images/products/potato-skins.jpg"));
    }

    [Fact]
    public void ResolveProductImageUrl_UnmappedProduct_ReturnsNull_WhenStoredPathMissing()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        var resolved = resolver.ResolveProductImageUrl(2, "/images/products/shrimp-tempura.jpg");

        Assert.Null(resolved);
    }

    [Fact]
    public void ResolveProductImageUrl_DoesNotEmitLegacyDinaPath_ForPlaceholderAssets()
    {
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(FindWebRootPath()));

        var resolved = resolver.ResolveProductImageUrl(99, "/dina-html/images/menu/marinated-grilled-shrimp.jpg");

        Assert.Null(resolved);
    }

    [Fact]
    public void ResolveProductImageUrl_AcceptsNonPlaceholderLocalAsset()
    {
        var webRoot = FindWebRootPath();
        var resolver = new PublicMediaUrlResolver(new TestWebHostEnvironment(webRoot));
        var orphanPath = Path.Combine(webRoot, "images", "menu", "catalog-orphan-test.jpg");
        try
        {
            // Minimal JPEG without placeholder text markers.
            File.WriteAllBytes(orphanPath, [0xFF, 0xD8, 0xFF, 0xD9]);

            var resolved = resolver.ResolveProductImageUrl(99999, "/images/menu/catalog-orphan-test.jpg");

            Assert.Equal("/images/menu/catalog-orphan-test.jpg", resolved);
        }
        finally
        {
            if (File.Exists(orphanPath))
            {
                File.Delete(orphanPath);
            }
        }
    }

    private static string FindWebRootPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate wwwroot for resolver tests.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = webRootPath;
            WebRootFileProvider = new NullFileProvider();
            ContentRootFileProvider = new NullFileProvider();
        }

        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
