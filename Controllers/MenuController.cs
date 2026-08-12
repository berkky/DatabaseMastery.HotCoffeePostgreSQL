using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    public class MenuController : Controller
    {
        private readonly IPublicRestaurantService _publicRestaurantService;

        public MenuController(IPublicRestaurantService publicRestaurantService)
        {
            _publicRestaurantService = publicRestaurantService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = await _publicRestaurantService.GetLandingAsync(cancellationToken);
            return View(model);
        }
    }
}
