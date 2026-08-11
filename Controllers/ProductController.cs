using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using DatabaseMastery.HotCoffeePostgreSQL.Services.CategoryServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> ProductList()
        {
            var values = await _productService.GetAllProductsAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            await PopulateCategoriesSelectList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            BusinessRequestNormalizer.TrimCreateProduct(createProductDto);
            ModelState.Clear();
            if (!TryValidateModel(createProductDto))
            {
                await PopulateCategoriesSelectList(createProductDto.CategoryId);
                return View(createProductDto);
            }

            if (!await _categoryService.CategoryExistsAsync(createProductDto.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(createProductDto.CategoryId),
                    "Seçilen kategori bulunamadı.");
                await PopulateCategoriesSelectList(createProductDto.CategoryId);
                return View(createProductDto);
            }

            await _productService.CreateProductAsync(createProductDto);
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!await _productService.ProductExistsAsync(id))
            {
                return NotFound();
            }

            await PopulateCategoriesSelectList();

            var values = await _productService.GetProductByIdAsync(id);
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            if (id != updateProductDto.ProductId)
            {
                return BadRequest();
            }

            BusinessRequestNormalizer.TrimUpdateProduct(updateProductDto);
            ModelState.Clear();
            if (!TryValidateModel(updateProductDto))
            {
                await PopulateCategoriesSelectList(updateProductDto.CategoryId);
                return View(MapToGetProductByIdDto(updateProductDto));
            }

            if (!await _productService.ProductExistsAsync(id))
            {
                return NotFound();
            }

            if (!await _categoryService.CategoryExistsAsync(updateProductDto.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(updateProductDto.CategoryId),
                    "Seçilen kategori bulunamadı.");
                await PopulateCategoriesSelectList(updateProductDto.CategoryId);
                return View(MapToGetProductByIdDto(updateProductDto));
            }

            var updated = await _productService.UpdateProductAsync(updateProductDto);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction("ProductList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _productService.DeleteProductAsync(id);

            switch (result.Status)
            {
                case DeleteOperationStatus.Deleted:
                    return RedirectToAction("ProductList");
                case DeleteOperationStatus.BlockedByDependencies:
                    TempData[AdminFlashMessages.WarningKey] =
                        "Bu ürün silinemez çünkü ürüne bağlı müşteri yorumları bulunuyor.";
                    return RedirectToAction("ProductList");
                default:
                    return NotFound();
            }
        }

        private async Task PopulateCategoriesSelectList(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "CategoryId",
                "CategoryName",
                selectedCategoryId);
        }

        private static GetProductByIdDto MapToGetProductByIdDto(UpdateProductDto dto)
        {
            return new GetProductByIdDto
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Status = dto.Status,
                Price = dto.Price,
                CategoryId = dto.CategoryId
            };
        }
    }
}
