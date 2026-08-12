using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.PublicRestaurantDtos;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant
{
    public sealed class PublicRestaurantService : IPublicRestaurantService
    {
        private readonly AppDbContext _context;
        private readonly IPublicMediaUrlResolver _mediaUrlResolver;

        public PublicRestaurantService(
            AppDbContext context,
            IPublicMediaUrlResolver mediaUrlResolver)
        {
            _context = context;
            _mediaUrlResolver = mediaUrlResolver;
        }

        public async Task<PublicLandingDto> GetLandingAsync(CancellationToken cancellationToken = default)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status && p.Category.CategoryStatus)
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.ProductId)
                .Select(p => new PublicProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                product.ImageUrl = _mediaUrlResolver.ResolveProductImageUrl(
                    product.ProductId,
                    product.ImageUrl);
            }

            var categories = products
                .GroupBy(p => new { p.CategoryId, p.CategoryName })
                .OrderBy(g => g.Key.CategoryId)
                .Select(g => new PublicCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Products = g.ToList()
                })
                .ToList();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.ReviewId)
                .Take(6)
                .Select(r => new PublicReviewDto
                {
                    CustomerName = r.CustomerName,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    ProductName = r.Product != null ? r.Product.ProductName : null
                })
                .ToListAsync(cancellationToken);

            return new PublicLandingDto
            {
                Categories = categories,
                RecentPublishedReviews = reviews,
                ActiveCategoryCount = categories.Count,
                ActiveProductCount = products.Count,
                HeroStageImageUrl = _mediaUrlResolver.ResolveHeroStageImageUrl()
            };
        }
    }
}
