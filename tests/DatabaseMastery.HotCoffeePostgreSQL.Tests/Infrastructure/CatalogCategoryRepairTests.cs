using System.Text.Json;
using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Infrastructure;

/// <summary>
/// STEP 10.5 controlled Product.CategoryId repair against the development database.
/// Gated by HC_CATEGORY_REPAIR=1. Snapshot + transaction + row-by-row verification.
/// </summary>
public sealed class CatalogCategoryRepairTests
{
    private readonly ITestOutputHelper _output;

    public CatalogCategoryRepairTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Product seed CategoryId → canonical CategoryName (CategoryQueryExample order).
    /// ProductQueryExample used a different CategoryId numbering than Category inserts.
    /// </summary>
    private static readonly IReadOnlyDictionary<int, string> SeedCategoryIdToCanonicalName =
        new Dictionary<int, string>
        {
            [1] = "Başlangıçlar",
            [2] = "Çorbalar",                 // seed block is soups
            [3] = "Salatalar",
            [4] = "Ana Yemekler",             // seed block is mains
            [5] = "Izgara & Et Yemekleri",    // seed block is grill/kebab
            [6] = "Makarnalar",               // seed block is pasta
            [7] = "Pizza",                    // seed block is pizza
            [8] = "Tatlılar",
            [9] = "İçecekler",
            [10] = "Kahve & Çay",
        };

    [Fact]
    public void RepairProductCategories_WhenAuthorized_AppliesDeterministicRemap()
    {
        if (Environment.GetEnvironmentVariable("HC_CATEGORY_REPAIR") != "1")
        {
            return;
        }

        var root = FindRepositoryRoot();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets("53d33993-9df1-4f26-a2a2-9e4417d3dd49")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string missing.");

        var seedPath = Path.Combine(root, "Requirements", "ExampleDatas", "ProductQueryExample.txt");
        var seedRows = ParseSeedProductRows(seedPath);
        Assert.Equal(147, seedRows.Count);

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        AssertCounts(connection, categories: 10, products: 147, reservations: 52, reviews: 500);

        var categoriesByName = LoadCategoriesByName(connection);
        Assert.Equal(10, categoriesByName.Count);
        foreach (var name in SeedCategoryIdToCanonicalName.Values)
        {
            Assert.True(categoriesByName.ContainsKey(name), $"Missing category '{name}'.");
        }

        var products = LoadProducts(connection);
        Assert.Equal(147, products.Count);

        var snapshotPath = Path.Combine(Path.GetTempPath(), "hc-category-before-snapshot.tsv");
        File.WriteAllLines(
            snapshotPath,
            products.Select(p => $"{p.ProductId}\t{p.ProductName}\t{p.CategoryId}"));
        _output.WriteLine($"SNAPSHOT={snapshotPath}");

        var seedByKey = seedRows.ToDictionary(
            s => SeedKey(s.ProductName, s.ImageUrl),
            s => s.SeedCategoryId,
            StringComparer.Ordinal);

        var planned = new List<(ProductRow Product, int FromCategoryId, int ToCategoryId, string CanonicalName)>();
        foreach (var product in products)
        {
            var key = SeedKey(product.ProductName, product.ImageUrl);
            Assert.True(seedByKey.TryGetValue(key, out var seedCategoryId),
                $"Product '{product.ProductName}' ({product.ImageUrl}) missing from authoritative seed.");

            var canonicalName = SeedCategoryIdToCanonicalName[seedCategoryId];
            var toCategoryId = categoriesByName[canonicalName];
            if (product.CategoryId != toCategoryId)
            {
                planned.Add((product, product.CategoryId, toCategoryId, canonicalName));
            }
        }

        _output.WriteLine($"PLANNED_CHANGES={planned.Count}");
        Assert.True(planned.Count > 0, "Expected category mismatches to repair.");

        using (var tx = connection.BeginTransaction())
        {
            foreach (var change in planned)
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = """
                    UPDATE "Products"
                    SET "CategoryId" = @toCategoryId
                    WHERE "ProductId" = @productId
                      AND "ProductName" = @productName
                      AND "CategoryId" = @fromCategoryId
                      AND "Description" = @description
                      AND "Price" = @price
                      AND "Status" = @status
                      AND "ImageUrl" = @imageUrl
                    """;
                cmd.Parameters.AddWithValue("toCategoryId", change.ToCategoryId);
                cmd.Parameters.AddWithValue("productId", change.Product.ProductId);
                cmd.Parameters.AddWithValue("productName", change.Product.ProductName);
                cmd.Parameters.AddWithValue("fromCategoryId", change.FromCategoryId);
                cmd.Parameters.AddWithValue("description", change.Product.Description);
                cmd.Parameters.AddWithValue("price", change.Product.Price);
                cmd.Parameters.AddWithValue("status", change.Product.Status);
                cmd.Parameters.AddWithValue("imageUrl", change.Product.ImageUrl);

                var affected = cmd.ExecuteNonQuery();
                Assert.Equal(1, affected);
            }

            tx.Commit();
        }

        var after = LoadProducts(connection);
        Assert.Equal(147, after.Count);

        var unexpected = 0;
        var changed = 0;
        foreach (var before in products)
        {
            var row = after.Single(p => p.ProductId == before.ProductId);
            if (row.ProductName != before.ProductName
                || row.Description != before.Description
                || row.Price != before.Price
                || row.Status != before.Status
                || row.ImageUrl != before.ImageUrl)
            {
                unexpected++;
                continue;
            }

            var seedCategoryId = seedByKey[SeedKey(before.ProductName, before.ImageUrl)];
            var expectedCategoryId = categoriesByName[SeedCategoryIdToCanonicalName[seedCategoryId]];
            if (row.CategoryId != expectedCategoryId)
            {
                unexpected++;
                continue;
            }

            if (row.CategoryId != before.CategoryId)
            {
                changed++;
            }
        }

        Assert.Equal(0, unexpected);
        Assert.Equal(planned.Count, changed);
        AssertCounts(connection, categories: 10, products: 147, reservations: 52, reviews: 500);

        using (var orphanCmd = connection.CreateCommand())
        {
            orphanCmd.CommandText = """
                SELECT COUNT(*) FROM "Products" p
                LEFT JOIN "Categories" c ON c."CategoryId" = p."CategoryId"
                WHERE c."CategoryId" IS NULL
                """;
            Assert.Equal(0L, (long)orphanCmd.ExecuteScalar()!);
        }

        _output.WriteLine($"CATEGORY_CHANGED={changed}");
        _output.WriteLine("REPAIR_OK");
    }

    private static string SeedKey(string productName, string imageUrl)
        => productName + "\u001f" + imageUrl;

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

            var match = System.Text.RegularExpressions.Regex.Match(
                trimmed,
                @"^\('((?:\\'|[^'])*)','((?:\\'|[^'])*)','((?:\\'|[^'])*)',\s*(true|false)\s*,\s*([0-9.]+)\s*,\s*(\d+)\s*\)\s*[,;]?\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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

    private static void AssertCounts(
        NpgsqlConnection connection,
        int categories,
        int products,
        int reservations,
        int reviews)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM "Categories"),
                (SELECT COUNT(*) FROM "Products"),
                (SELECT COUNT(*) FROM "Reservations"),
                (SELECT COUNT(*) FROM "Reviews")
            """;
        using var reader = cmd.ExecuteReader();
        reader.Read();
        Assert.Equal(categories, reader.GetInt64(0));
        Assert.Equal(products, reader.GetInt64(1));
        Assert.Equal(reservations, reader.GetInt64(2));
        Assert.Equal(reviews, reader.GetInt64(3));
    }

    private static Dictionary<string, int> LoadCategoriesByName(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """SELECT "CategoryId", "CategoryName" FROM "Categories" ORDER BY "CategoryId" """;
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            map[reader.GetString(1)] = reader.GetInt32(0);
        }

        return map;
    }

    private static List<ProductRow> LoadProducts(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT "ProductId", "ProductName", "Description", "Price", "Status", "ImageUrl", "CategoryId"
            FROM "Products"
            ORDER BY "ProductId"
            """;
        var list = new List<ProductRow>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ProductRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDecimal(3),
                reader.GetBoolean(4),
                reader.GetString(5),
                reader.GetInt32(6)));
        }

        return list;
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

    private sealed record SeedProductRow(string ProductName, string ImageUrl, int SeedCategoryId);

    private sealed record ProductRow(
        int ProductId,
        string ProductName,
        string Description,
        decimal Price,
        bool Status,
        string ImageUrl,
        int CategoryId);
}
