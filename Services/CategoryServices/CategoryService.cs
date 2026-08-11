using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(x => x.CategoryId == id);
        }

        public async Task CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var value = EntityMappers.ToEntity(createCategoryDto);
            await _context.Categories.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task<DeleteOperationResult> DeleteCategoryAsync(int id)
        {
            var value = await _context.Categories.FindAsync(id);
            if (value == null)
            {
                return DeleteOperationResult.NotFound;
            }

            if (await _context.Products.AnyAsync(p => p.CategoryId == id))
            {
                return DeleteOperationResult.BlockedByDependencies;
            }

            _context.Categories.Remove(value);

            try
            {
                await _context.SaveChangesAsync();
                return DeleteOperationResult.Deleted;
            }
            catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsForeignKeyViolation(ex))
            {
                return DeleteOperationResult.BlockedByDependencies;
            }
        }

        public async Task<List<ResultCategoryDto>> GetAllCategoriesAsync()
        {
            var values = await _context.Categories
                .AsNoTracking()
                .ToListAsync();
            return EntityMappers.ToResultCategoryDtos(values);
        }

        public async Task<GetCategoryByIdDto> GetCategoryByIdAsync(int id)
        {
            var value = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CategoryId == id);
            return value == null ? null! : EntityMappers.ToGetByIdDto(value);
        }

        public async Task<bool> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(updateCategoryDto.CategoryId);
            if (category == null)
            {
                return false;
            }

            category.CategoryName = updateCategoryDto.CategoryName;
            category.CategoryImageUrl = updateCategoryDto.CategoryImageUrl;
            category.CategoryStatus = updateCategoryDto.CategoryStatus;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
