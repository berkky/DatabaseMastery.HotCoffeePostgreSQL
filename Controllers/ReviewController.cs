using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReviewServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
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
            var products = await _productService.GetAllProductsAsync();
            ViewBag.Products = new SelectList(
                products.Where(p => p.Status),
                "ProductId", "ProductName"
            );
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewDto createReviewDto)
        {
            createReviewDto.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            await _reviewService.CreateReviewAsync(createReviewDto);
            return RedirectToAction("ReviewList");
        }
    }
}
