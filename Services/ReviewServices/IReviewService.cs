using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices
{
    public interface IReviewService
    {
        Task<List<ResultReviewDto>> GetAllReviewsAsync();
        Task<GetReviewByIdDto?> GetReviewByIdAsync(int id);
        Task<bool> ReviewExistsAsync(int id);
        Task CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<bool> UpdateReviewAsync(UpdateReviewDto updateReviewDto);
        Task<bool> PublishReviewAsync(int id);
        Task<bool> HideReviewAsync(int id);
        Task<DeleteOperationResult> DeleteReviewAsync(int id);
    }
}
