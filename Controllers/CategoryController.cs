using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> CategoryList()
        {
            var values = await _categoryService.GetAllCategoriesAsync();
            return View(values);
        }

        public async Task<IActionResult> CategoryCardList()
        {
            var values = await _categoryService.GetAllCategoriesAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            BusinessRequestNormalizer.TrimCreateCategory(createCategoryDto);
            ModelState.Clear();
            if (!TryValidateModel(createCategoryDto))
            {
                return View(createCategoryDto);
            }

            await _categoryService.CreateCategoryAsync(createCategoryDto);
            return RedirectToAction("CategoryCardList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _categoryService.DeleteCategoryAsync(id);

            switch (result.Status)
            {
                case DeleteOperationStatus.Deleted:
                    return RedirectToAction("CategoryCardList");
                case DeleteOperationStatus.BlockedByDependencies:
                    TempData[AdminFlashMessages.WarningKey] =
                        "Bu kategori silinemez çünkü kategoriye bağlı ürünler bulunuyor.";
                    return RedirectToAction("CategoryCardList");
                default:
                    return NotFound();
            }
        }

        public async Task<IActionResult> UpdateCategory(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!await _categoryService.CategoryExistsAsync(id))
            {
                return NotFound();
            }

            var value = await _categoryService.GetCategoryByIdAsync(id);
            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            if (id != updateCategoryDto.CategoryId)
            {
                return BadRequest();
            }

            BusinessRequestNormalizer.TrimUpdateCategory(updateCategoryDto);
            ModelState.Clear();
            if (!TryValidateModel(updateCategoryDto))
            {
                return View(MapToGetCategoryByIdDto(updateCategoryDto));
            }

            if (!await _categoryService.CategoryExistsAsync(id))
            {
                return NotFound();
            }

            var updated = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction("CategoryCardList");
        }

        private static GetCategoryByIdDto MapToGetCategoryByIdDto(UpdateCategoryDto dto)
        {
            return new GetCategoryByIdDto
            {
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                CategoryImageUrl = dto.CategoryImageUrl,
                CategoryStatus = dto.CategoryStatus
            };
        }
    }
}
