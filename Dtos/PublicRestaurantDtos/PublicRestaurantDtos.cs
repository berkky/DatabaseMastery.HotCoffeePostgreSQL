namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.PublicRestaurantDtos
{
    public sealed class PublicLandingDto
    {
        public IReadOnlyList<PublicCategoryDto> Categories { get; init; } = Array.Empty<PublicCategoryDto>();
        public IReadOnlyList<PublicReviewDto> RecentPublishedReviews { get; init; } = Array.Empty<PublicReviewDto>();
        public int ActiveProductCount { get; init; }
        public int ActiveCategoryCount { get; init; }
        public string? HeroStageImageUrl { get; init; }
    }

    public sealed class PublicCategoryDto
    {
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public IReadOnlyList<PublicProductDto> Products { get; init; } = Array.Empty<PublicProductDto>();
    }

    public sealed class PublicProductDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
    }

    public sealed class PublicReviewDto
    {
        public string CustomerName { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
        public int Rating { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? ProductName { get; init; }
    }
}
