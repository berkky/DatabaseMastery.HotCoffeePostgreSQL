using System.Text.Json;
using System.Text.RegularExpressions;
using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class Step105CatalogIntegrityTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private static readonly IReadOnlyDictionary<int, string> SeedCategoryIdToCanonicalName =
        new Dictionary<int, string>
        {
            [1] = "Başlangıçlar",
            [2] = "Çorbalar",
            [3] = "Salatalar",
            [4] = "Ana Yemekler",
            [5] = "Izgara & Et Yemekleri",
            [6] = "Makarnalar",
            [7] = "Pizza",
            [8] = "Tatlılar",
            [9] = "İçecekler",
            [10] = "Kahve & Çay",
        };

    private readonly HotCoffeeWebApplicationFactory _factory;

    public Step105CatalogIntegrityTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AuthoritativeSeed_DefinesCanonicalCategoryNames_ForAll147Products()
    {
        var root = FindRepositoryRoot();
        var seedPath = Path.Combine(root, "Requirements", "ExampleDatas", "ProductQueryExample.txt");
        var rows = ParseSeedProductRows(seedPath);

        Assert.Equal(147, rows.Count);
        Assert.Contains(rows, r => r.ProductName == "Mercimek Çorbası" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Çorbalar");
        Assert.Contains(rows, r => r.ProductName == "Izgara Tavuk" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Ana Yemekler");
        Assert.Contains(rows, r => r.ProductName == "Fırın Somon" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Ana Yemekler");
        Assert.Contains(rows, r => r.ProductName == "Dana Bonfile" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Ana Yemekler");
        Assert.Contains(rows, r => r.ProductName == "Adana Kebap" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Izgara & Et Yemekleri");
        Assert.Contains(rows, r => r.ProductName == "Spaghetti Bolognese" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Makarnalar");
        Assert.Contains(rows, r => r.ProductName == "Pizza Margherita" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Pizza");
        Assert.Contains(rows, r => r.ProductName == "Tiramisu" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Tatlılar");
        Assert.Contains(rows, r => r.ProductName == "Espresso" && SeedCategoryIdToCanonicalName[r.SeedCategoryId] == "Kahve & Çay");
    }

    [Fact]
    public void ProductMediaCatalog_HasNoGenuinePhotoMappings_AfterPlaceholderRemoval()
    {
        Assert.Empty(ProductMediaCatalog.VerifiedMappings);

        var placeholder = Path.Combine(FindRepositoryRoot(), "wwwroot", "images", "menu", "avocado-tomato.jpg");
        Assert.True(ProductMediaCatalog.IsTemplatePlaceholderAsset(placeholder));
    }

    [Fact]
    public void ImageGenerationManifest_Exists_WithOneEntryPerActiveProduct()
    {
        var path = Path.Combine(FindRepositoryRoot(), "docs", "menu-image-generation-manifest.json");
        Assert.True(File.Exists(path), $"Missing manifest at {path}");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        Assert.Equal("webp", root.GetProperty("format").GetString());
        Assert.Equal("/images/products/generated/", root.GetProperty("targetRoot").GetString());

        var products = root.GetProperty("products");
        Assert.Equal(147, products.GetArrayLength());

        foreach (var product in products.EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(product.GetProperty("productName").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(product.GetProperty("categoryName").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(product.GetProperty("slug").GetString()));
            Assert.StartsWith(
                "/images/products/generated/",
                product.GetProperty("targetFile").GetString(),
                StringComparison.Ordinal);
            Assert.EndsWith(".webp", product.GetProperty("targetFile").GetString(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CatalogCategoryMap_Exists_WithAuthoritativeConfidence()
    {
        var path = Path.Combine(FindRepositoryRoot(), "docs", "catalog-category-map.json");
        Assert.True(File.Exists(path), $"Missing map at {path}");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var products = doc.RootElement.GetProperty("products");
        Assert.Equal(147, products.GetArrayLength());

        var unresolved = 0;
        foreach (var product in products.EnumerateArray())
        {
            var confidence = product.GetProperty("confidence").GetString();
            if (confidence is not ("AUTHORITATIVE" or "STRONG_SOURCE"))
            {
                unresolved++;
            }
        }

        Assert.Equal(0, unresolved);
    }

    [Fact]
    public void RealDatabase_WhenConfigured_HasCorrectCategoryMembership()
    {
        if (Environment.GetEnvironmentVariable("HC_STEP105_VERIFY") != "1")
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(FindRepositoryRoot())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets("53d33993-9df1-4f26-a2a2-9e4417d3dd49")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string missing.");

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        AssertCategoryContains(connection, "Çorbalar", "Mercimek Çorbası");
        AssertCategoryContains(connection, "Ana Yemekler", "Izgara Tavuk");
        AssertCategoryContains(connection, "Ana Yemekler", "Fırın Somon");
        AssertCategoryContains(connection, "Ana Yemekler", "Dana Bonfile");
        AssertCategoryDoesNotContain(connection, "Çorbalar", "Izgara Tavuk");
        AssertCategoryDoesNotContain(connection, "Çorbalar", "Fırın Somon");
        AssertCategoryDoesNotContain(connection, "Çorbalar", "Dana Bonfile");
        AssertCategoryContains(connection, "Makarnalar", "Spaghetti Bolognese");
        AssertCategoryContains(connection, "Pizza", "Pizza Margherita");
        AssertCategoryContains(connection, "Tatlılar", "Tiramisu");
        AssertCategoryContains(connection, "Kahve & Çay", "Espresso");
        AssertCategoryContains(connection, "Izgara & Et Yemekleri", "Adana Kebap");
    }

    [Fact]
    public async Task PublicMenu_DoesNotEmitMenuImgPlaceholderMarkup()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Menu/Index");

        Assert.DoesNotContain("Menu IMG", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("400x400", html, StringComparison.Ordinal);
        Assert.Contains("hc-product-fallback", html, StringComparison.Ordinal);
        Assert.Contains("hc-motion-ready", await client.GetStringAsync("/js/hotcoffee-public.js"), StringComparison.Ordinal);
    }

    private static void AssertCategoryContains(NpgsqlConnection connection, string category, string product)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM "Products" p
            INNER JOIN "Categories" c ON c."CategoryId" = p."CategoryId"
            WHERE c."CategoryName" = @category AND p."ProductName" = @product
            """;
        cmd.Parameters.AddWithValue("category", category);
        cmd.Parameters.AddWithValue("product", product);
        Assert.Equal(1L, (long)cmd.ExecuteScalar()!);
    }

    private static void AssertCategoryDoesNotContain(NpgsqlConnection connection, string category, string product)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM "Products" p
            INNER JOIN "Categories" c ON c."CategoryId" = p."CategoryId"
            WHERE c."CategoryName" = @category AND p."ProductName" = @product
            """;
        cmd.Parameters.AddWithValue("category", category);
        cmd.Parameters.AddWithValue("product", product);
        Assert.Equal(0L, (long)cmd.ExecuteScalar()!);
    }

    private static List<SeedProductRow> ParseSeedProductRows(string path)
    {
        var list = new List<SeedProductRow>();
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("('", StringComparison.Ordinal))
            {
                continue;
            }

            var match = Regex.Match(
                trimmed,
                @"^\('((?:\\'|[^'])*)','((?:\\'|[^'])*)','((?:\\'|[^'])*)',\s*(true|false)\s*,\s*([0-9.]+)\s*,\s*(\d+)\s*\)\s*[,;]?\s*$",
                RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                continue;
            }

            list.Add(new SeedProductRow(
                match.Groups[1].Value,
                match.Groups[3].Value,
                int.Parse(match.Groups[6].Value)));
        }

        return list;
    }

    private sealed record SeedProductRow(string ProductName, string ImageUrl, int SeedCategoryId);

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
}
