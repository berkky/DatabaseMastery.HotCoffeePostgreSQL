using DatabaseMastery.HotCoffeePostgreSQL.Dtos.AdminAnalyticsDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics
{
    public interface IAdminAnalyticsService
    {
        Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RecentReservationDto>> GetTodayReservationsAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RecentReviewDto>> GetRecentPublishedReviewsAsync(
            int take = 4,
            CancellationToken cancellationToken = default);

        Task<StatisticsOverviewDto> GetStatisticsOverviewAsync(CancellationToken cancellationToken = default);
    }
}
