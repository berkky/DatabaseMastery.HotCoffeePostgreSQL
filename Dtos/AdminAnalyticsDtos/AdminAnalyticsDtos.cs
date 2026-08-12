using DatabaseMastery.HotCoffeePostgreSQL.Domain;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.AdminAnalyticsDtos
{
    public sealed class DashboardOverviewDto
    {
        public int TotalReservations { get; init; }
        public int PendingReservations { get; init; }
        public int ConfirmedReservations { get; init; }
        public int CancelledReservations { get; init; }
        public int TodayReservations { get; init; }
        public int ThisMonthConfirmed { get; init; }
        public int ThisMonthCancelled { get; init; }
        public int ThisWeekReservations { get; init; }
        public int ThisMonthReservations { get; init; }
        public int UpcomingReservations { get; init; }
        public int TotalGuestCount { get; init; }
        public int ThisWeekGuestCount { get; init; }
        public int TotalProducts { get; init; }
        public int ActiveCategoryCount { get; init; }
        public double AverageRating { get; init; }
        public int TotalReviews { get; init; }
        public double ConfirmationRatePercent { get; init; }
        public double CancelRatePercent { get; init; }
        public double SatisfactionPercent { get; init; }
        public TimeOnly? NextTodayReservationTime { get; init; }
    }

    public sealed class RecentReviewDto
    {
        public int ReviewId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string Comment { get; init; } = string.Empty;
        public int Rating { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? ProductName { get; init; }
    }

    public sealed class RecentReservationDto
    {
        public int ReservationId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateOnly ReservationDate { get; init; }
        public TimeOnly ReservationTime { get; init; }
        public int GuestCount { get; init; }
        public ReservationStatus Status { get; init; }
        public string? Description { get; init; }
    }

    public sealed class ReservationStatusSummaryDto
    {
        public int Total { get; init; }
        public int Confirmed { get; init; }
        public int Pending { get; init; }
        public int Cancelled { get; init; }
        public double ConfirmedRatePercent { get; init; }
        public double PendingRatePercent { get; init; }
        public double CancelledRatePercent { get; init; }
    }

    public sealed class CategoryAnalyticsDto
    {
        public string CategoryName { get; init; } = string.Empty;
        public int ProductCount { get; init; }
        public double AvgPrice { get; init; }
        public int RelativePercent { get; init; }
    }

    public sealed class RatingDistributionDto
    {
        public int TotalReviews { get; init; }
        public double AverageRating { get; init; }
        public int Star5 { get; init; }
        public int Star4 { get; init; }
        public int Star3 { get; init; }
        public int Star2 { get; init; }
        public int Star1 { get; init; }
        public double Pct5 { get; init; }
        public double Pct4 { get; init; }
        public double Pct3 { get; init; }
        public double Pct2 { get; init; }
        public double Pct1 { get; init; }
    }

    public sealed class ProductAnalyticsDto
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int ReviewCount { get; init; }
        public double AverageRating { get; init; }
    }

    public sealed class HeatmapCellDto
    {
        public int HourIndex { get; init; }
        public int DayIndex { get; init; }
        public int Count { get; init; }
    }

    public sealed class ReservationHeatmapDto
    {
        public int[,] Cells { get; init; } = new int[6, 7];
        public int MaxValue { get; init; } = 1;
        public string PeakDayLabel { get; init; } = "—";
        public string PeakHourLabel { get; init; } = "—";
    }

    public sealed class StatisticsOverviewDto
    {
        public int TotalReservations { get; init; }
        public int PendingReservations { get; init; }
        public int ConfirmedReservations { get; init; }
        public int CancelledReservations { get; init; }
        public double ConfirmationRatePercent { get; init; }
        public double CancelRatePercent { get; init; }
        public int TotalGuestCount { get; init; }
        public int ThisWeekConfirmedGuestCount { get; init; }
        public int TodayReservations { get; init; }
        public int ThisWeekReservations { get; init; }
        public int ThisMonthReservations { get; init; }
        public double AverageGroupSize { get; init; }
        public double DailyAverageReservationsLast30Days { get; init; }
        public int TotalReviews { get; init; }
        public int PublishedReviews { get; init; }
        public int HiddenReviews { get; init; }
        public double AverageRating { get; init; }
        public int FiveStarReviews { get; init; }
        public double FiveStarPercent { get; init; }
        public int ActiveProducts { get; init; }
        public int TotalCategories { get; init; }
        public int ActiveCategories { get; init; }
        public ReservationStatusSummaryDto StatusSummary { get; init; } = new();
        public IReadOnlyList<CategoryAnalyticsDto> TopCategories { get; init; } = Array.Empty<CategoryAnalyticsDto>();
        public RatingDistributionDto RatingDistribution { get; init; } = new();
        public IReadOnlyList<ProductAnalyticsDto> TopProductsByReviewCount { get; init; } = Array.Empty<ProductAnalyticsDto>();
        public ReservationHeatmapDto Heatmap { get; init; } = new();
    }
}
