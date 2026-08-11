using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;

        public ReviewController(IReviewService reviewService, IProductService productService)
        {
            _reviewService = reviewService;
            _productService = productService;
        }

        public async Task<IActionResult> ReviewList()
        {
            var values = await _reviewService.GetAllReviewsAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReview()
        {
            await PopulateProductsSelectList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewDto createReviewDto)
        {
            BusinessRequestNormalizer.TrimCreateReview(createReviewDto);
            createReviewDto.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            createReviewDto.Status = true;

            ModelState.Clear();
            if (!TryValidateModel(createReviewDto))
            {
                await PopulateProductsSelectList(createReviewDto.ProductId);
                return View(createReviewDto);
            }

            if (!await _productService.ProductExistsAsync(createReviewDto.ProductId))
            {
                ModelState.AddModelError(
                    nameof(createReviewDto.ProductId),
                    "Seçilen ürün bulunamadı.");
                await PopulateProductsSelectList(createReviewDto.ProductId);
                return View(createReviewDto);
            }

            await _reviewService.CreateReviewAsync(createReviewDto);
            TempData[AdminFlashMessages.SuccessKey] = "Yorum başarıyla eklendi.";
            return RedirectToAction("ReviewList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReview(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var model = new UpdateReviewDto
            {
                ReviewId = review.ReviewId,
                CustomerName = review.CustomerName,
                Comment = review.Comment,
                Rating = review.Rating,
                ProductId = review.ProductId
            };

            await PopulateProductsSelectList(model.ProductId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto updateReviewDto)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (id != updateReviewDto.ReviewId)
            {
                return BadRequest();
            }

            BusinessRequestNormalizer.TrimUpdateReview(updateReviewDto);
            ModelState.Clear();
            if (!TryValidateModel(updateReviewDto))
            {
                await PopulateProductsSelectList(updateReviewDto.ProductId);
                return View(updateReviewDto);
            }

            if (!await _reviewService.ReviewExistsAsync(id))
            {
                return NotFound();
            }

            if (!await _productService.ProductExistsAsync(updateReviewDto.ProductId))
            {
                ModelState.AddModelError(
                    nameof(updateReviewDto.ProductId),
                    "Seçilen ürün bulunamadı.");
                await PopulateProductsSelectList(updateReviewDto.ProductId);
                return View(updateReviewDto);
            }

            var updated = await _reviewService.UpdateReviewAsync(updateReviewDto);
            if (!updated)
            {
                return NotFound();
            }

            TempData[AdminFlashMessages.SuccessKey] = "Yorum başarıyla güncellendi.";
            return RedirectToAction("ReviewList");
        }

        [HttpPost]
        public async Task<IActionResult> PublishReview(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var published = await _reviewService.PublishReviewAsync(id);
            if (!published)
            {
                return NotFound();
            }

            TempData[AdminFlashMessages.SuccessKey] = "Yorum yayınlandı.";
            return RedirectToAction("ReviewList");
        }

        [HttpPost]
        public async Task<IActionResult> HideReview(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var hidden = await _reviewService.HideReviewAsync(id);
            if (!hidden)
            {
                return NotFound();
            }

            TempData[AdminFlashMessages.SuccessKey] = "Yorum gizlendi.";
            return RedirectToAction("ReviewList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _reviewService.DeleteReviewAsync(id);
            if (result.Status == DeleteOperationStatus.NotFound)
            {
                return NotFound();
            }

            TempData[AdminFlashMessages.SuccessKey] = "Yorum silindi.";
            return RedirectToAction("ReviewList");
        }

        private async Task PopulateProductsSelectList(int? selectedProductId = null)
        {
            var products = await _productService.GetAllProductsAsync();
            ViewBag.Products = new SelectList(
                products.Where(p => p.Status),
                "ProductId",
                "ProductName",
                selectedProductId);
        }
    }
}
