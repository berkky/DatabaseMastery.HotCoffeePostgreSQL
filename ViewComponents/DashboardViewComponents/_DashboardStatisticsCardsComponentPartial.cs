using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.DashboardViewComponents
{
    public class _DashboardStatisticsCardsComponentPartial : ViewComponent
    {
        private readonly IAdminAnalyticsService _analytics;

        public _DashboardStatisticsCardsComponentPartial(IAdminAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var overview = await _analytics.GetDashboardOverviewAsync();
            return View(overview);
        }
    }
}
