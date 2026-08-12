using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Infrastructure;

/// <summary>
/// Read-only forensic audit against the configured development database.
/// </summary>
public sealed class MenuImageForensicAuditTests
{
    private readonly ITestOutputHelper _output;

    public MenuImageForensicAuditTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ForensicAudit_WhenRealDatabaseConfigured_ReportsAggregateImageStatistics()
    {
        if (Environment.GetEnvironmentVariable("HC_MENU_IMAGE_AUDIT") != "1")
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
            throw new InvalidOperationException("Connection string missing for forensic audit.");
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

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p."ProductId", p."ImageUrl"
            FROM "Products" p
            WHERE p."Status" = true
            """;

        var total = 0;
        var empty = 0;
        var legacy = 0;
        var imagesMenu = 0;
        var otherLocal = 0;
        var external = 0;
        var exactResolved = 0;
        var fallback = 0;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            total++;
            var productId = reader.GetInt32(0);
            var stored = reader.IsDBNull(1) ? null : reader.GetString(1);
            if (string.IsNullOrWhiteSpace(stored))
            {
                empty++;
                fallback++;
                continue;
            }

            if (stored.StartsWith("/dina-html/", StringComparison.OrdinalIgnoreCase))
            {
                legacy++;
            }
            else if (stored.StartsWith("/images/menu/", StringComparison.OrdinalIgnoreCase))
            {
                imagesMenu++;
            }
            else if (stored.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || stored.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                external++;
            }
            else if (stored.StartsWith("/", StringComparison.Ordinal))
            {
                otherLocal++;
            }

            if (resolver.ResolveProductImageUrl(productId, stored) is not null)
            {
                exactResolved++;
            }
            else
            {
                fallback++;
            }
        }

        _output.WriteLine($"ACTIVE_PRODUCTS={total}");
        _output.WriteLine($"EMPTY={empty}");
        _output.WriteLine($"LEGACY_DINA={legacy}");
        _output.WriteLine($"IMAGES_MENU_PATH={imagesMenu}");
        _output.WriteLine($"OTHER_LOCAL={otherLocal}");
        _output.WriteLine($"EXTERNAL={external}");
        _output.WriteLine($"EXACT_RESOLVED={exactResolved}");
        _output.WriteLine($"FALLBACK={fallback}");
        _output.WriteLine($"RECOVERED_MENU_FILES={Directory.GetFiles(Path.Combine(webRoot, "images", "menu")).Length}");

        Assert.True(total > 0, "Expected active products in forensic audit database.");
    }

    [Fact]
    public void ExportProductInventory_WhenRequested_WritesReadOnlySnapshot()
    {
        if (Environment.GetEnvironmentVariable("HC_EXPORT_PRODUCTS") != "1")
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
            throw new InvalidOperationException("Connection string missing for export.");
        }

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p."ProductId", p."ProductName", c."CategoryName", p."ImageUrl"
            FROM "Products" p
            INNER JOIN "Categories" c ON c."CategoryId" = p."CategoryId"
            WHERE p."Status" = true
            ORDER BY p."ProductId"
            """;

        var lines = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var productId = reader.GetInt32(0);
            var productName = reader.GetString(1).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
            var categoryName = reader.GetString(2).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
            var imageUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            lines.Add($"{productId}\t{productName}\t{categoryName}\t{imageUrl}");
        }

        var exportPath = Path.Combine(Path.GetTempPath(), "hc-products-export.tsv");
        File.WriteAllLines(exportPath, lines);
        _output.WriteLine($"EXPORT_PATH={exportPath}");
        _output.WriteLine($"EXPORT_COUNT={lines.Count}");
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
