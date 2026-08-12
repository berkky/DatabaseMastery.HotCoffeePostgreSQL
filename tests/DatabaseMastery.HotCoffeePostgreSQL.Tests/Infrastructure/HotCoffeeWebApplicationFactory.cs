using System.Net.Http;
using System.Text.RegularExpressions;
using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Configuration;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests;

public sealed class HotCoffeeWebApplicationFactory : WebApplicationFactory<Program>
{
    public string TestUsername { get; } = $"test-admin-{Guid.NewGuid():N}";
    public string TestPassword { get; } = $"TestPw-{Guid.NewGuid():N}!9";
    public string DatabaseName { get; } = $"HotCoffeeTests-{Guid.NewGuid():N}";

    public int CategoryWithProductsId { get; private set; }
    public int EmptyCategoryId { get; private set; }
    public int ProductWithReviewsId { get; private set; }
    public int ProductWithoutReviewsId { get; private set; }
    public int PublishedReviewId { get; private set; }
    public int HiddenReviewId { get; private set; }
    public DateTime HiddenReviewCreatedAt { get; private set; }
    public int PendingReservationId { get; private set; }

    public DateTimeOffset FixedUtcNow { get; } =
        new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    public DateOnly BusinessToday { get; private set; }

    private bool _seeded;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminAuth:Username"] = "factory-admin",
                ["AdminAuth:Password"] = "factory-password"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // EF Core 8+/10 registers provider configuration separately; remove all
            // AppDbContext option registrations so only InMemory remains.
            RemoveAppDbContextRegistrations(services);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));

            var fixedProvider = new FakeTimeProvider(FixedUtcNow);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(fixedProvider);
            services.RemoveAll<IBusinessClock>();
            services.AddSingleton<IBusinessClock>(new BusinessClock(fixedProvider));
            BusinessToday = new BusinessClock(fixedProvider).Today;

            services.PostConfigure<AdminAuthOptions>(options =>
            {
                options.Username = TestUsername;
                options.Password = TestPassword;
            });

            services.PostConfigure<CookieAuthenticationOptions>(AuthSchemes.HotCoffeeAdmin, options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.PostConfigure<RateLimitingOptions>(options =>
            {
                options.AdminLoginPermitLimit = 10_000;
                options.AdminLoginWindowMinutes = 5;
                options.PublicReservationPermitLimit = 10_000;
                options.PublicReservationWindowMinutes = 10;
            });
        });
    }

    private static void RemoveAppDbContextRegistrations(IServiceCollection services)
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(AppDbContext)
                || d.ServiceType == typeof(DbContextOptions)
                || d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                || (d.ServiceType.IsGenericType
                    && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
                    && d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext))
                || d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
            .ToList();

        foreach (var descriptor in toRemove)
        {
            services.Remove(descriptor);
        }
    }

    protected override void ConfigureClient(HttpClient client)
    {
        EnsureSeeded();
        base.ConfigureClient(client);
    }

    public void EnsureSeeded()
    {
        if (_seeded)
        {
            return;
        }

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Categories.Any())
        {
            var categoryA = new Category
            {
                CategoryName = "Category A",
                CategoryImageUrl = "/images/a.jpg",
                CategoryStatus = true
            };
            var categoryB = new Category
            {
                CategoryName = "Category B",
                CategoryImageUrl = "/images/b.jpg",
                CategoryStatus = true
            };
            db.Categories.AddRange(categoryA, categoryB);
            db.SaveChanges();

            var productWithReviews = new Product
            {
                ProductName = "Product With Reviews",
                Description = "Has reviews",
                ImageUrl = "/images/p10.jpg",
                Status = true,
                Price = 25.00m,
                CategoryId = categoryA.CategoryId
            };
            var productWithoutReviews = new Product
            {
                ProductName = "Product Without Reviews",
                Description = "No reviews",
                ImageUrl = "/images/p11.jpg",
                Status = true,
                Price = 15.00m,
                CategoryId = categoryA.CategoryId
            };
            db.Products.AddRange(productWithReviews, productWithoutReviews);
            db.SaveChanges();

            HiddenReviewCreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            db.Reviews.AddRange(
                new Review
                {
                    CustomerName = "Published Customer",
                    Comment = "Great",
                    Rating = 5,
                    CreatedAt = HiddenReviewCreatedAt,
                    Status = true,
                    ProductId = productWithReviews.ProductId
                },
                new Review
                {
                    CustomerName = "Hidden Customer",
                    Comment = "Hidden comment",
                    Rating = 3,
                    CreatedAt = HiddenReviewCreatedAt,
                    Status = false,
                    ProductId = productWithReviews.ProductId
                });

            db.Reservations.Add(new Reservation
            {
                Name = "Pending Guest",
                Phone = "+905551112233",
                Email = "guest@example.com",
                ReservationDate = BusinessToday.AddDays(2),
                ReservationTime = new TimeOnly(19, 0),
                GuestCount = 2,
                Status = ReservationStatus.Pending,
                Description = "Test reservation"
            });
            db.SaveChanges();
        }

        // Ensure BusinessToday is available even if seed ran before ConfigureTestServices completed clock wiring
        if (BusinessToday == default)
        {
            BusinessToday = new BusinessClock(new FakeTimeProvider(FixedUtcNow)).Today;
        }

        CategoryWithProductsId = db.Categories.Single(c => c.CategoryName == "Category A").CategoryId;
        EmptyCategoryId = db.Categories.Single(c => c.CategoryName == "Category B").CategoryId;
        ProductWithReviewsId = db.Products.Single(p => p.ProductName == "Product With Reviews").ProductId;
        ProductWithoutReviewsId = db.Products.Single(p => p.ProductName == "Product Without Reviews").ProductId;
        PublishedReviewId = db.Reviews.Single(r => r.CustomerName == "Published Customer").ReviewId;
        HiddenReviewId = db.Reviews.Single(r => r.CustomerName == "Hidden Customer").ReviewId;
        HiddenReviewCreatedAt = db.Reviews.Single(r => r.CustomerName == "Hidden Customer").CreatedAt;
        PendingReservationId = db.Reservations.Single(r => r.Name == "Pending Guest").ReservationId;
        _seeded = true;
    }

    public static string? ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        match = Regex.Match(
            html,
            @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        EnsureSeeded();

        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html)
            ?? throw new InvalidOperationException("Antiforgery token missing on login page.");

        var response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = TestUsername,
                ["Password"] = TestPassword,
                ["__RequestVerificationToken"] = token
            }));

        if ((int)response.StatusCode is not (302 or 303))
        {
            throw new InvalidOperationException($"Login failed with status {(int)response.StatusCode}.");
        }

        return client;
    }
}
