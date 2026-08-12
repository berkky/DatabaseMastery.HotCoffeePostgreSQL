using System.Net;
using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step103ProductMediaHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step103ProductMediaHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HeroAsset_Serves200_WithImageContentType()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/images/menu/menu-hero-3d.jpg");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("image/", response.Content.Headers.ContentType?.MediaType ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublicMenu_DoesNotEmitPlaceholderProductPhotos()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.DoesNotContain("/images/menu/products/", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-fallback-mark", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/dina-html/", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/images/products/", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicMenu_StillUsesFallbackForUnmappedFactoryProducts()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.Contains("hc-product-fallback-mark", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/images/products/", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/dina-html/", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublicMenu_PreservesCinematicHeroFromStep102()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.Contains("data-hc-hero", html, StringComparison.Ordinal);
        Assert.Contains("hc-hero-cinematic", html, StringComparison.Ordinal);
        Assert.Contains("/images/menu/menu-hero-3d.jpg", html, StringComparison.Ordinal);
    }

    [Fact]
    public void RealDatabaseResolver_WhenConfigured_ResolvesZeroPlaceholderPhotos()
    {
        if (Environment.GetEnvironmentVariable("HC_STEP103_HTML") != "1")
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(FindRepositoryRoot())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets("53d33993-9df1-4f26-a2a2-9e4417d3dd49")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string missing for STEP 10.3 resolver audit.");
        }

        var webRoot = Path.Combine(FindRepositoryRoot(), "wwwroot");
        var resolver = new PublicMediaUrlResolver(new AuditWebHostEnvironment(webRoot));

        using var connection = new NpgsqlConnection(connectionString);
        try
        {
            connection.Open();
        }
        catch (PostgresException ex) when (ex.SqlState is "28P01" or "3D000")
        {
            return;
        }

        using (var countCommand = connection.CreateCommand())
        {
            countCommand.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM "Categories"),
                    (SELECT COUNT(*) FROM "Products" WHERE "Status" = true),
                    (SELECT COUNT(*) FROM "Reservations"),
                    (SELECT COUNT(*) FROM "Reviews")
                """;
            using var reader = countCommand.ExecuteReader();
            reader.Read();
            Assert.Equal(10, reader.GetInt64(0));
            Assert.Equal(147, reader.GetInt64(1));
            Assert.Equal(52, reader.GetInt64(2));
            Assert.Equal(500, reader.GetInt64(3));
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p."ProductId", p."ImageUrl"
            FROM "Products" p
            WHERE p."Status" = true
            ORDER BY p."ProductId"
            """;

        var mapped = 0;
        var fallback = 0;

        using var productReader = command.ExecuteReader();
        while (productReader.Read())
        {
            var productId = productReader.GetInt32(0);
            var stored = productReader.IsDBNull(1) ? null : productReader.GetString(1);
            var resolved = resolver.ResolveProductImageUrl(productId, stored);

            if (resolved is null)
            {
                fallback++;
            }
            else
            {
                mapped++;
            }
        }

        Assert.Equal(0, mapped);
        Assert.Equal(147, fallback);
        Assert.Empty(ProductMediaCatalog.VerifiedMappings);
    }

    [Fact]
    public void ProductMediaCatalog_ContainsNoPlaceholderMappings()
    {
        Assert.Empty(ProductMediaCatalog.VerifiedMappings);
        Assert.Equal("/images/products/generated/", ProductMediaCatalog.GeneratedProductsRoot);
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DatabaseMastery.HotCoffeePostgreSQL.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed class AuditWebHostEnvironment : IWebHostEnvironment
    {
        public AuditWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = webRootPath;
        }

        public string ApplicationName { get; set; } = "Audit";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
