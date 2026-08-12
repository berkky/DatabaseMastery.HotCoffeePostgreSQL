using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Services;

public class AdminAnalyticsServiceTests
{
    [Fact]
    public async Task EmptyDatabase_ReturnsZeroSafeOverview()
    {
        await using var db = CreateEmptyContext();
        var clock = new BusinessClock(new FakeTimeProvider(
            new DateTimeOffset(2026, 8, 11, 12, 0, 0, TimeSpan.Zero)));
        var service = new AdminAnalyticsService(db, clock);

        var dashboard = await service.GetDashboardOverviewAsync();
        var statistics = await service.GetStatisticsOverviewAsync();
        var today = await service.GetTodayReservationsAsync();
        var reviews = await service.GetRecentPublishedReviewsAsync();

        Assert.Equal(0, dashboard.TotalReservations);
        Assert.Equal(0, dashboard.AverageRating);
        Assert.Equal(0, dashboard.TotalReviews);
        Assert.Equal(0, dashboard.ConfirmationRatePercent);
        Assert.Empty(today);
        Assert.Empty(reviews);

        Assert.Equal(0, statistics.TotalReservations);
        Assert.Equal(0, statistics.AverageRating);
        Assert.Equal(0, statistics.AverageGroupSize);
        Assert.Equal(0, statistics.RatingDistribution.Star5);
        Assert.Empty(statistics.TopProductsByReviewCount);
    }

    [Fact]
    public async Task SeededDatabase_ComputesTruthfulAggregates()
    {
        await using var factory = new HotCoffeeWebApplicationFactory();
        factory.EnsureSeeded();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IBusinessClock>();

        db.Reservations.AddRange(
            new Reservation
            {
                Name = "Today A",
                Phone = "+905551110001",
                Email = "a@example.com",
                ReservationDate = clock.Today,
                ReservationTime = new TimeOnly(18, 30),
                GuestCount = 4,
                Status = ReservationStatus.Confirmed,
                Description = null
            },
            new Reservation
            {
                Name = "Today B",
                Phone = "+905551110002",
                Email = "b@example.com",
                ReservationDate = clock.Today,
                ReservationTime = new TimeOnly(20, 0),
                GuestCount = 2,
                Status = ReservationStatus.Pending,
                Description = null
            },
            new Reservation
            {
                Name = "Cancelled Guest",
                Phone = "+905551110003",
                Email = "c@example.com",
                ReservationDate = clock.Today.AddDays(-1),
                ReservationTime = new TimeOnly(19, 0),
                GuestCount = 3,
                Status = ReservationStatus.Cancelled,
                Description = null
            });
        await db.SaveChangesAsync();

        var service = scope.ServiceProvider.GetRequiredService<IAdminAnalyticsService>();

        var dashboard = await service.GetDashboardOverviewAsync();
        var statistics = await service.GetStatisticsOverviewAsync();
        var todayList = await service.GetTodayReservationsAsync();
        var recentReviews = await service.GetRecentPublishedReviewsAsync();

        Assert.Equal(4, dashboard.TotalReservations); // 1 seeded pending + 3 added
        Assert.Equal(1, dashboard.ConfirmedReservations);
        Assert.Equal(2, dashboard.PendingReservations);
        Assert.Equal(1, dashboard.CancelledReservations);
        Assert.Equal(2, dashboard.TodayReservations);
        Assert.Equal(new TimeOnly(18, 30), dashboard.NextTodayReservationTime);
        Assert.Equal(11, dashboard.TotalGuestCount); // 2 + 4 + 2 + 3
        Assert.Equal(4.0, dashboard.AverageRating); // (5+3)/2
        Assert.Equal(2, dashboard.TotalReviews);

        Assert.Equal(2, todayList.Count);
        Assert.Equal("Today A", todayList[0].Name);
        Assert.Equal(ReservationStatus.Confirmed, todayList[0].Status);

        Assert.Equal(dashboard.TotalReservations, statistics.TotalReservations);
        Assert.Equal(1, statistics.ConfirmedReservations);
        Assert.Equal(2, statistics.PendingReservations);
        Assert.Equal(1, statistics.CancelledReservations);
        Assert.Equal(2.8, statistics.AverageGroupSize); // 11/4
        Assert.Equal(1, statistics.PublishedReviews);
        Assert.Equal(1, statistics.HiddenReviews);
        Assert.Equal(2, statistics.ActiveProducts);
        Assert.Equal(2, statistics.ActiveCategories);
        Assert.Single(statistics.TopProductsByReviewCount);
        Assert.Equal("Product With Reviews", statistics.TopProductsByReviewCount[0].ProductName);
        Assert.Equal(2, statistics.TopProductsByReviewCount[0].ReviewCount);

        Assert.Single(recentReviews);
        Assert.Equal("Published Customer", recentReviews[0].CustomerName);
        Assert.DoesNotContain(recentReviews, r => r.CustomerName == "Hidden Customer");
    }

    private static AppDbContext CreateEmptyContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"EmptyAnalytics-{Guid.NewGuid():N}")
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
