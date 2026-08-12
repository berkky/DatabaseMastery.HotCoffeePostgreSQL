using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.DashboardViewComponents
{
    public class _DashboardLastReservationsComponentPartial : ViewComponent
    {
        private readonly IAdminAnalyticsService _analytics;

        public _DashboardLastReservationsComponentPartial(IAdminAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _analytics.GetTodayReservationsAsync();
            return View(values);
        }
    }
}
