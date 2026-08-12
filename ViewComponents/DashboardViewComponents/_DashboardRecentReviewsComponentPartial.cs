using DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.DashboardViewComponents
{
    public class _DashboardRecentReviewsComponentPartial : ViewComponent
    {
        private readonly IAdminAnalyticsService _analytics;

        public _DashboardRecentReviewsComponentPartial(IAdminAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var reviews = await _analytics.GetRecentPublishedReviewsAsync(4);
            return View(reviews);
        }
    }
}
