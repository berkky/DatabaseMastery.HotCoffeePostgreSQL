using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;

namespace DatabaseMastery.HotCoffeePostgreSQL.Mapping
{
    public static class EntityMappers
    {
        public static Category ToEntity(CreateCategoryDto dto) => new()
        {
            CategoryName = dto.CategoryName,
            CategoryImageUrl = dto.CategoryImageUrl,
            CategoryStatus = dto.CategoryStatus
        };

        public static ResultCategoryDto ToResultDto(Category entity) => new()
        {
            CategoryId = entity.CategoryId,
            CategoryName = entity.CategoryName,
            CategoryImageUrl = entity.CategoryImageUrl,
            CategoryStatus = entity.CategoryStatus
        };

        public static GetCategoryByIdDto ToGetByIdDto(Category entity) => new()
        {
            CategoryId = entity.CategoryId,
            CategoryName = entity.CategoryName,
            CategoryImageUrl = entity.CategoryImageUrl,
            CategoryStatus = entity.CategoryStatus
        };

        public static List<ResultCategoryDto> ToResultCategoryDtos(IEnumerable<Category> entities) =>
            entities.Select(ToResultDto).ToList();

        public static Product ToEntity(CreateProductDto dto) => new()
        {
            ProductName = dto.ProductName,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Status = dto.Status,
            Price = dto.Price,
            CategoryId = dto.CategoryId
        };

        public static ResultProductDto ToResultDto(Product entity) => new()
        {
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Status = entity.Status,
            Price = entity.Price,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.CategoryName ?? string.Empty
        };

        public static GetProductByIdDto ToGetByIdDto(Product entity) => new()
        {
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Status = entity.Status,
            Price = entity.Price,
            CategoryId = entity.CategoryId
        };

        public static List<ResultProductDto> ToResultProductDtos(IEnumerable<Product> entities) =>
            entities.Select(ToResultDto).ToList();

        public static Reservation ToEntity(CreateReservationDto dto) => new()
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            ReservationDate = dto.ReservationDate!.Value,
            ReservationTime = dto.ReservationTime!.Value,
            GuestCount = dto.GuestCount,
            Status = ReservationStatus.Pending,
            Description = dto.Description
        };

        public static ResultReservationDto ToResultDto(Reservation entity) => new()
        {
            ReservationId = entity.ReservationId,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            ReservationDate = entity.ReservationDate,
            ReservationTime = entity.ReservationTime,
            GuestCount = entity.GuestCount,
            Status = entity.Status,
            Description = entity.Description
        };

        public static GetReservationByIdDto ToGetByIdDto(Reservation entity) => new()
        {
            ReservationId = entity.ReservationId,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            ReservationDate = entity.ReservationDate,
            ReservationTime = entity.ReservationTime,
            GuestCount = entity.GuestCount,
            Status = entity.Status,
            Description = entity.Description
        };

        public static List<ResultReservationDto> ToResultReservationDtos(IEnumerable<Reservation> entities) =>
            entities.Select(ToResultDto).ToList();

        public static Review ToEntity(CreateReviewDto dto) => new()
        {
            CustomerName = dto.CustomerName,
            Comment = dto.Comment,
            Rating = dto.Rating,
            CreatedAt = dto.CreatedAt,
            Status = dto.Status,
            ProductId = dto.ProductId
        };

        public static ResultReviewDto ToResultDto(Review entity) => new()
        {
            ReviewId = entity.ReviewId,
            CustomerName = entity.CustomerName,
            Comment = entity.Comment,
            Rating = entity.Rating,
            CreatedAt = entity.CreatedAt,
            Status = entity.Status,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.ProductName ?? string.Empty
        };

        public static GetReviewByIdDto ToGetByIdDto(Review entity) => new()
        {
            ReviewId = entity.ReviewId,
            CustomerName = entity.CustomerName,
            Comment = entity.Comment,
            Rating = entity.Rating,
            CreatedAt = entity.CreatedAt,
            Status = entity.Status,
            ProductId = entity.ProductId
        };

        public static List<ResultReviewDto> ToResultReviewDtos(IEnumerable<Review> entities) =>
            entities.Select(ToResultDto).ToList();
    }
}
