using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.StatisticsViewComponents
{
    public class _StatisticsTopProductsComponentPartial : ViewComponent
    {
        private readonly IAdminAnalyticsService _analytics;

        public _StatisticsTopProductsComponentPartial(IAdminAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var overview = await _analytics.GetStatisticsOverviewAsync();
            return View(overview.TopProductsByReviewCount);
        }
    }
}
