using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<ResultCategoryDto>> GetAllCategoriesAsync();
        Task<GetCategoryByIdDto> GetCategoryByIdAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<bool> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);
        Task<DeleteOperationResult> DeleteCategoryAsync(int id);
    }
}
