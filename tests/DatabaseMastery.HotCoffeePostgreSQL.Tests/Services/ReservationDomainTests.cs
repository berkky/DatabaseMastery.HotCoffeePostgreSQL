using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Services;

public class ReservationDomainTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public ReservationDomainTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeeded();
    }

    [Fact]
    public async Task CreateReservation_AssignsPending_AndPreservesDateTime()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices.IReservationService>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Context.AppDbContext>();

        var date = _factory.BusinessToday.AddDays(2);
        var time = new TimeOnly(19, 30);

        await service.CreateReservationAsync(new CreateReservationDto
        {
            Name = "Canonical Guest",
            Phone = "+905551234567",
            Email = "canonical@example.com",
            ReservationDate = date,
            ReservationTime = time,
            GuestCount = 3,
            Description = "domain test"
        });

        var entity = await db.Reservations.AsNoTracking()
            .SingleAsync(r => r.Name == "Canonical Guest");
        Assert.Equal(ReservationStatus.Pending, entity.Status);
        Assert.Equal(date, entity.ReservationDate);
        Assert.Equal(time, entity.ReservationTime);
    }

    [Theory]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Cancelled)]
    public async Task SetReservationStatus_AssignsCanonicalEnum(ReservationStatus status)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices.IReservationService>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Context.AppDbContext>();

        Assert.True(await service.SetReservationStatusAsync(_factory.PendingReservationId, status));
        var entity = await db.Reservations.AsNoTracking()
            .SingleAsync(r => r.ReservationId == _factory.PendingReservationId);
        Assert.Equal(status, entity.Status);
    }

    [Fact]
    public void PastDate_IsRejected_UsingBusinessClock()
    {
        var clock = new BusinessClock(new FakeTimeProvider(_factory.FixedUtcNow));
        var modelState = new ModelStateDictionary();
        ReservationRequestRules.ValidateDateNotInPast(
            modelState,
            clock.Today.AddDays(-1),
            clock.Today);
        Assert.False(modelState.IsValid);
    }

    [Fact]
    public void Today_IsAccepted_UsingBusinessClock()
    {
        var clock = new BusinessClock(new FakeTimeProvider(_factory.FixedUtcNow));
        var modelState = new ModelStateDictionary();
        ReservationRequestRules.ValidateDateNotInPast(
            modelState,
            clock.Today,
            clock.Today);
        Assert.True(modelState.IsValid);
    }

    [Fact]
    public void CreateReservationDto_HasNoStatusProperty()
    {
        Assert.Null(typeof(CreateReservationDto).GetProperty("Status"));
    }

    [Fact]
    public void EntityMapper_Create_ForcesPending()
    {
        var entity = EntityMappers.ToEntity(new CreateReservationDto
        {
            Name = "X",
            Phone = "+905551111111",
            Email = "x@example.com",
            ReservationDate = _factory.BusinessToday.AddDays(1),
            ReservationTime = new TimeOnly(12, 0),
            GuestCount = 2
        });
        Assert.Equal(ReservationStatus.Pending, entity.Status);
    }
}

public class ReviewUtcCreationTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public ReviewUtcCreationTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeeded();
    }

    [Fact]
    public void TimeProvider_ProducesUtcKind()
    {
        var provider = _factory.Services.GetRequiredService<TimeProvider>();
        var utc = provider.GetUtcNow().UtcDateTime;
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public async Task ReviewUpdate_StillPreservesCreatedAt()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Context.AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices.IReviewService>();

        var before = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.HiddenReviewId);
        Assert.True(await service.UpdateReviewAsync(new DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos.UpdateReviewDto
        {
            ReviewId = _factory.HiddenReviewId,
            CustomerName = "Utc Guard",
            Comment = "still preserved",
            Rating = 4,
            ProductId = _factory.ProductWithReviewsId
        }));

        var after = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.HiddenReviewId);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.False(after.Status);
    }
}
