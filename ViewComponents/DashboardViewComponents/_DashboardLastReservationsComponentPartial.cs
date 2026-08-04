using DatabaseMastery.HotCoffeePostgreSQL.Services.DashboardServices;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.DashboardViewComponents
{
    public class _DashboardLastReservationsComponentPartial : ViewComponent
    {
        private readonly IDashboardService _dashboardService;

        public _DashboardLastReservationsComponentPartial(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _dashboardService.GetTodayReservationListAsync();
            return View(values);
        }
    }
}
