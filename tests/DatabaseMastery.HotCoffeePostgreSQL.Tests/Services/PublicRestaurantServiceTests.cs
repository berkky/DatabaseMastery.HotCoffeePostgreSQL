using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Services;

public class PublicRestaurantServiceTests
{
    [Fact]
    public async Task GetLanding_HidesInactiveProductsAndCategories_AndHiddenReviews()
    {
        await using var factory = new HotCoffeeWebApplicationFactory();
        factory.EnsureSeeded();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var inactiveCategory = new Category
        {
            CategoryName = "Inactive Category",
            CategoryImageUrl = "/images/x.jpg",
            CategoryStatus = false
        };
        db.Categories.Add(inactiveCategory);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new Product
            {
                ProductName = "Inactive Product",
                Description = "Should hide",
                ImageUrl = "/images/i.jpg",
                Status = false,
                Price = 9m,
                CategoryId = factory.CategoryWithProductsId
            },
            new Product
            {
                ProductName = "Product In Inactive Category",
                Description = "Should hide",
                ImageUrl = "/images/j.jpg",
                Status = true,
                Price = 11m,
                CategoryId = inactiveCategory.CategoryId
            });

        db.Reviews.Add(new Review
        {
            CustomerName = "Fresh Published",
            Comment = "Visible public review",
            Rating = 5,
            CreatedAt = DateTime.UtcNow,
            Status = true,
            ProductId = factory.ProductWithReviewsId
        });
        await db.SaveChangesAsync();

        var service = scope.ServiceProvider.GetRequiredService<IPublicRestaurantService>();
        var landing = await service.GetLandingAsync();

        Assert.Contains(landing.Categories.SelectMany(c => c.Products), p => p.ProductName == "Product With Reviews");
        Assert.DoesNotContain(landing.Categories.SelectMany(c => c.Products), p => p.ProductName == "Inactive Product");
        Assert.DoesNotContain(landing.Categories.SelectMany(c => c.Products), p => p.ProductName == "Product In Inactive Category");
        Assert.DoesNotContain(landing.Categories, c => c.CategoryName == "Inactive Category");

        Assert.Contains(landing.RecentPublishedReviews, r => r.CustomerName == "Fresh Published");
        Assert.Contains(landing.RecentPublishedReviews, r => r.CustomerName == "Published Customer");
        Assert.DoesNotContain(landing.RecentPublishedReviews, r => r.CustomerName == "Hidden Customer");
    }
}
