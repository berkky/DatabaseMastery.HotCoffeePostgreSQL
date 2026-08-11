using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Validation;

public class DtoValidationTests
{
    private static IList<ValidationResult> Validate(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Product_InvalidPrice_Fails(decimal price)
    {
        var dto = ValidProduct();
        dto.Price = price;
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateProductDto.Price)));
    }

    [Fact]
    public void Product_ZeroCategoryId_Fails()
    {
        var dto = ValidProduct();
        dto.CategoryId = 0;
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateProductDto.CategoryId)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Reservation_InvalidGuestCount_Fails(int guests)
    {
        var dto = ValidReservation();
        dto.GuestCount = guests;
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateReservationDto.GuestCount)));
    }

    [Fact]
    public void Reservation_MalformedEmail_Fails()
    {
        var dto = ValidReservation();
        dto.Email = "not-an-email";
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateReservationDto.Email)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Review_InvalidRating_Fails(int rating)
    {
        var dto = ValidReview();
        dto.Rating = rating;
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateReviewDto.Rating)));
    }

    [Fact]
    public void Review_ZeroProductId_Fails()
    {
        var dto = ValidReview();
        dto.ProductId = 0;
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateReviewDto.ProductId)));
    }

    [Fact]
    public void Category_EmptyName_Fails()
    {
        var dto = new CreateCategoryDto
        {
            CategoryName = string.Empty,
            CategoryImageUrl = "/images/a.jpg",
            CategoryStatus = true
        };
        Assert.Contains(Validate(dto), r => r.MemberNames.Contains(nameof(CreateCategoryDto.CategoryName)));
    }

    private static CreateProductDto ValidProduct() => new()
    {
        ProductName = "Latte",
        Description = "Coffee",
        ImageUrl = "/images/latte.jpg",
        Status = true,
        Price = 10m,
        CategoryId = 1
    };

    private static CreateReservationDto ValidReservation() => new()
    {
        Name = "Guest",
        Phone = "+905551112233",
        Email = "guest@example.com",
        ReservationDate = DateTime.Today.AddDays(1),
        ReservationTime = TimeSpan.FromHours(19),
        GuestCount = 2,
        Description = "note"
    };

    private static CreateReviewDto ValidReview() => new()
    {
        CustomerName = "Customer",
        Comment = "Nice",
        Rating = 4,
        ProductId = 1,
        CreatedAt = DateTime.UtcNow,
        Status = true
    };
}
