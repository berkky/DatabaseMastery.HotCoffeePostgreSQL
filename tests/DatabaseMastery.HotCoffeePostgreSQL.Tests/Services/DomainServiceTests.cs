using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Services;

public class DomainServiceTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public DomainServiceTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeeded();
    }

    [Fact]
    public async Task CategoryDelete_WithProducts_IsBlocked()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();
        var result = await service.DeleteCategoryAsync(_factory.CategoryWithProductsId);
        Assert.Equal(DeleteOperationStatus.BlockedByDependencies, result.Status);
    }

    [Fact]
    public async Task CategoryDelete_Missing_IsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();
        var result = await service.DeleteCategoryAsync(2147483647);
        Assert.Equal(DeleteOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ProductDelete_WithReviews_IsBlocked()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();
        var result = await service.DeleteProductAsync(_factory.ProductWithReviewsId);
        Assert.Equal(DeleteOperationStatus.BlockedByDependencies, result.Status);
    }

    [Fact]
    public async Task ProductDelete_WithoutReviews_IsDeleted()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();

        var leaf = new DatabaseMastery.HotCoffeePostgreSQL.Entities.Product
        {
            ProductName = "Leaf Product",
            Description = "temp",
            ImageUrl = "/images/leaf.jpg",
            Status = true,
            Price = 9m,
            CategoryId = _factory.CategoryWithProductsId
        };
        db.Products.Add(leaf);
        await db.SaveChangesAsync();

        var result = await service.DeleteProductAsync(leaf.ProductId);
        Assert.Equal(DeleteOperationStatus.Deleted, result.Status);
        Assert.False(await db.Products.AnyAsync(p => p.ProductId == leaf.ProductId));
    }

    [Fact]
    public async Task ReviewDelete_Missing_IsNotFound()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IReviewService>();
        var result = await service.DeleteReviewAsync(2147483647);
        Assert.Equal(DeleteOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ReviewDelete_Existing_IsDeleted()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IReviewService>();

        var review = new DatabaseMastery.HotCoffeePostgreSQL.Entities.Review
        {
            CustomerName = "Temp Reviewer",
            Comment = "temp",
            Rating = 4,
            CreatedAt = DateTime.UtcNow,
            Status = true,
            ProductId = _factory.ProductWithReviewsId
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        var result = await service.DeleteReviewAsync(review.ReviewId);
        Assert.Equal(DeleteOperationStatus.Deleted, result.Status);
    }

    [Fact]
    public async Task CategoryUpdate_AssignsOnlyEditableFields()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        var category = new DatabaseMastery.HotCoffeePostgreSQL.Entities.Category
        {
            CategoryName = "Temp Category",
            CategoryImageUrl = "/images/temp.jpg",
            CategoryStatus = true
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var ok = await service.UpdateCategoryAsync(new UpdateCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = "Temp Category Updated",
            CategoryImageUrl = "/images/temp2.jpg",
            CategoryStatus = false
        });
        Assert.True(ok);

        var entity = await db.Categories.AsNoTracking().SingleAsync(c => c.CategoryId == category.CategoryId);
        Assert.Equal("Temp Category Updated", entity.CategoryName);
        Assert.Equal("/images/temp2.jpg", entity.CategoryImageUrl);
        Assert.False(entity.CategoryStatus);
    }

    [Fact]
    public async Task ProductUpdate_AssignsAllowedFields()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();

        var product = new DatabaseMastery.HotCoffeePostgreSQL.Entities.Product
        {
            ProductName = "Updatable Product",
            Description = "desc",
            ImageUrl = "/images/u.jpg",
            Status = true,
            Price = 12m,
            CategoryId = _factory.CategoryWithProductsId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var ok = await service.UpdateProductAsync(new UpdateProductDto
        {
            ProductId = product.ProductId,
            ProductName = "Renamed Product",
            Description = "Updated desc",
            ImageUrl = "/images/new.jpg",
            Status = false,
            Price = 42.5m,
            CategoryId = _factory.EmptyCategoryId
        });
        Assert.True(ok);

        var entity = await db.Products.AsNoTracking().SingleAsync(p => p.ProductId == product.ProductId);
        Assert.Equal("Renamed Product", entity.ProductName);
        Assert.Equal(42.5m, entity.Price);
        Assert.Equal(_factory.EmptyCategoryId, entity.CategoryId);
        Assert.False(entity.Status);
    }

    [Fact]
    public async Task ReservationUpdate_AssignsAllowedFields()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

        var reservation = new DatabaseMastery.HotCoffeePostgreSQL.Entities.Reservation
        {
            Name = "Editable Guest",
            Phone = "+905551110000",
            Email = "edit@example.com",
            ReservationDate = DateTime.SpecifyKind(DateTime.Today.AddDays(3), DateTimeKind.Utc),
            ReservationTime = new TimeSpan(18, 0, 0),
            GuestCount = 2,
            Status = "Beklemede",
            Description = "note"
        };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        var ok = await service.UpdateReservationAsync(new UpdateReservationDto
        {
            ReservationId = reservation.ReservationId,
            Name = "Updated Guest",
            Phone = "+905559998877",
            Email = "updated@example.com",
            ReservationDate = DateTime.SpecifyKind(DateTime.Today.AddDays(5), DateTimeKind.Utc),
            ReservationTime = new TimeSpan(20, 30, 0),
            GuestCount = 4,
            Status = "Onaylandı",
            Description = "updated note"
        });
        Assert.True(ok);

        var entity = await db.Reservations.AsNoTracking().SingleAsync(r => r.ReservationId == reservation.ReservationId);
        Assert.Equal("Updated Guest", entity.Name);
        Assert.Equal("Onaylandı", entity.Status);
        Assert.Equal(4, entity.GuestCount);
    }

    [Fact]
    public async Task ReviewUpdate_PreservesCreatedAtAndStatus()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IReviewService>();

        var before = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.HiddenReviewId);
        Assert.False(before.Status);

        var ok = await service.UpdateReviewAsync(new UpdateReviewDto
        {
            ReviewId = _factory.HiddenReviewId,
            CustomerName = "Edited Name",
            Comment = "Edited comment",
            Rating = 2,
            ProductId = _factory.ProductWithoutReviewsId
        });
        Assert.True(ok);

        var after = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.HiddenReviewId);
        Assert.Equal("Edited Name", after.CustomerName);
        Assert.Equal("Edited comment", after.Comment);
        Assert.Equal(2, after.Rating);
        Assert.Equal(_factory.ProductWithoutReviewsId, after.ProductId);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.False(after.Status);
    }

    [Fact]
    public async Task PublishAndHide_ChangeOnlyStatus()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IReviewService>();

        var before = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.PublishedReviewId);

        Assert.True(await service.HideReviewAsync(_factory.PublishedReviewId));
        var hidden = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.PublishedReviewId);
        Assert.False(hidden.Status);
        Assert.Equal(before.CustomerName, hidden.CustomerName);
        Assert.Equal(before.Comment, hidden.Comment);
        Assert.Equal(before.Rating, hidden.Rating);
        Assert.Equal(before.ProductId, hidden.ProductId);
        Assert.Equal(before.CreatedAt, hidden.CreatedAt);

        Assert.True(await service.PublishReviewAsync(_factory.PublishedReviewId));
        var published = await db.Reviews.AsNoTracking().SingleAsync(r => r.ReviewId == _factory.PublishedReviewId);
        Assert.True(published.Status);
        Assert.Equal(before.CustomerName, published.CustomerName);
        Assert.Equal(before.CreatedAt, published.CreatedAt);
    }
}
