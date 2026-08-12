using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.StatisticsViewComponents
{
    public class _StatisticsRatingDistComponentPartial : ViewComponent
    {
        private readonly IAdminAnalyticsService _analytics;

        public _StatisticsRatingDistComponentPartial(IAdminAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var overview = await _analytics.GetStatisticsOverviewAsync();
            return View(overview.RatingDistribution);
        }
    }
}
