using DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.MenuViewComponents
{
    public class _MenuListComponentPartial : ViewComponent
    {
        private readonly IPublicRestaurantService _publicRestaurantService;

        public _MenuListComponentPartial(IPublicRestaurantService publicRestaurantService)
        {
            _publicRestaurantService = publicRestaurantService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var landing = await _publicRestaurantService.GetLandingAsync();
            return View(landing);
        }
    }
}
