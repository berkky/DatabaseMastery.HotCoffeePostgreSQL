using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            var value = EntityMappers.ToEntity(createReviewDto);
            await _context.Reviews.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task<DeleteOperationResult> DeleteReviewAsync(int id)
        {
            var value = await _context.Reviews.FindAsync(id);
            if (value == null)
            {
                return DeleteOperationResult.NotFound;
            }

            _context.Reviews.Remove(value);
            await _context.SaveChangesAsync();
            return DeleteOperationResult.Deleted;
        }

        public async Task<List<ResultReviewDto>> GetAllReviewsAsync()
        {
            var values = await _context.Reviews
                .AsNoTracking()
                .Include(y => y.Product)
                .ToListAsync();
            return EntityMappers.ToResultReviewDtos(values);
        }

        public async Task<GetReviewByIdDto?> GetReviewByIdAsync(int id)
        {
            var value = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReviewId == id);
            if (value == null)
            {
                return null;
            }

            return EntityMappers.ToGetByIdDto(value);
        }

        public async Task<bool> ReviewExistsAsync(int id)
        {
            return await _context.Reviews.AnyAsync(x => x.ReviewId == id);
        }

        public async Task<bool> UpdateReviewAsync(UpdateReviewDto updateReviewDto)
        {
            var review = await _context.Reviews.FindAsync(updateReviewDto.ReviewId);
            if (review == null)
            {
                return false;
            }

            review.CustomerName = updateReviewDto.CustomerName;
            review.Comment = updateReviewDto.Comment;
            review.Rating = updateReviewDto.Rating;
            review.ProductId = updateReviewDto.ProductId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return false;
            }

            review.Status = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HideReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return false;
            }

            review.Status = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
