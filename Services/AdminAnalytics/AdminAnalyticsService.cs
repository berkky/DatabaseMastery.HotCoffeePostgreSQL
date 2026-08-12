using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.AdminAnalyticsDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAnalytics
{
    public sealed class AdminAnalyticsService : IAdminAnalyticsService
    {
        private static readonly string[] DayLabelsTr =
            { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };

        private static readonly int[] HourSlots = { 12, 14, 16, 18, 20, 22 };

        private readonly AppDbContext _context;
        private readonly IBusinessClock _businessClock;

        private DashboardOverviewDto? _dashboardCache;
        private StatisticsOverviewDto? _statisticsCache;
        private IReadOnlyList<RecentReservationDto>? _todayReservationsCache;
        private IReadOnlyList<RecentReviewDto>? _recentReviewsCache;

        public AdminAnalyticsService(AppDbContext context, IBusinessClock businessClock)
        {
            _context = context;
            _businessClock = businessClock;
        }

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(
            CancellationToken cancellationToken = default)
        {
            if (_dashboardCache is not null)
            {
                return _dashboardCache;
            }

            var today = _businessClock.Today;
            var weekStart = StartOfWeekMonday(today);
            var weekEndExclusive = weekStart.AddDays(7);
            var monthStart = new DateOnly(today.Year, today.Month, 1);

            var reservations = _context.Reservations.AsNoTracking();
            var reviews = _context.Reviews.AsNoTracking();
            var products = _context.Products.AsNoTracking();
            var categories = _context.Categories.AsNoTracking();

            var statusGroups = await reservations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int StatusCount(ReservationStatus status) =>
                statusGroups.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            var pending = StatusCount(ReservationStatus.Pending);
            var confirmed = StatusCount(ReservationStatus.Confirmed);
            var cancelled = StatusCount(ReservationStatus.Cancelled);
            var totalReservations = pending + confirmed + cancelled;

            var todayCount = await reservations.CountAsync(
                r => r.ReservationDate == today, cancellationToken);

            var nextTodayTime = await reservations
                .Where(r => r.ReservationDate == today && r.Status != ReservationStatus.Cancelled)
                .OrderBy(r => r.ReservationTime)
                .Select(r => (TimeOnly?)r.ReservationTime)
                .FirstOrDefaultAsync(cancellationToken);

            var monthRows = await reservations
                .Where(r => r.ReservationDate >= monthStart && r.ReservationDate <= today)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var thisMonthConfirmed = monthRows
                .FirstOrDefault(x => x.Status == ReservationStatus.Confirmed)?.Count ?? 0;
            var thisMonthCancelled = monthRows
                .FirstOrDefault(x => x.Status == ReservationStatus.Cancelled)?.Count ?? 0;
            var thisMonthReservations = monthRows.Sum(x => x.Count);

            var thisWeekReservations = await reservations.CountAsync(
                r => r.ReservationDate >= weekStart && r.ReservationDate < weekEndExclusive,
                cancellationToken);

            var upcoming = await reservations.CountAsync(
                r => r.ReservationDate >= today && r.Status != ReservationStatus.Cancelled,
                cancellationToken);

            var totalGuests = await reservations.SumAsync(r => (int?)r.GuestCount, cancellationToken) ?? 0;
            var thisWeekGuests = await reservations
                .Where(r => r.ReservationDate >= weekStart && r.ReservationDate < weekEndExclusive)
                .SumAsync(r => (int?)r.GuestCount, cancellationToken) ?? 0;

            var totalProducts = await products.CountAsync(cancellationToken);
            var activeCategories = await categories.CountAsync(c => c.CategoryStatus, cancellationToken);

            var reviewAgg = await reviews
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    Average = g.Average(r => (double)r.Rating)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var totalReviews = reviewAgg?.Count ?? 0;
            var averageRating = totalReviews == 0
                ? 0d
                : Math.Round(reviewAgg!.Average, 1);

            _dashboardCache = new DashboardOverviewDto
            {
                TotalReservations = totalReservations,
                PendingReservations = pending,
                ConfirmedReservations = confirmed,
                CancelledReservations = cancelled,
                TodayReservations = todayCount,
                ThisMonthConfirmed = thisMonthConfirmed,
                ThisMonthCancelled = thisMonthCancelled,
                ThisWeekReservations = thisWeekReservations,
                ThisMonthReservations = thisMonthReservations,
                UpcomingReservations = upcoming,
                TotalGuestCount = totalGuests,
                ThisWeekGuestCount = thisWeekGuests,
                TotalProducts = totalProducts,
                ActiveCategoryCount = activeCategories,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                ConfirmationRatePercent = Rate(confirmed, totalReservations),
                CancelRatePercent = Rate(cancelled, totalReservations),
                SatisfactionPercent = Math.Round(averageRating / 5d * 100d, 1),
                NextTodayReservationTime = nextTodayTime
            };

            return _dashboardCache;
        }

        public async Task<IReadOnlyList<RecentReservationDto>> GetTodayReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_todayReservationsCache is not null)
            {
                return _todayReservationsCache;
            }

            var today = _businessClock.Today;

            _todayReservationsCache = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.ReservationDate == today)
                .OrderBy(r => r.ReservationTime)
                .ThenBy(r => r.ReservationId)
                .Select(r => new RecentReservationDto
                {
                    ReservationId = r.ReservationId,
                    Name = r.Name,
                    Phone = r.Phone,
                    Email = r.Email,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    GuestCount = r.GuestCount,
                    Status = r.Status,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);

            return _todayReservationsCache;
        }

        public async Task<IReadOnlyList<RecentReviewDto>> GetRecentPublishedReviewsAsync(
            int take = 4,
            CancellationToken cancellationToken = default)
        {
            if (_recentReviewsCache is not null)
            {
                return _recentReviewsCache;
            }

            _recentReviewsCache = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.ReviewId)
                .Take(take)
                .Select(r => new RecentReviewDto
                {
                    ReviewId = r.ReviewId,
                    CustomerName = r.CustomerName,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    ProductName = r.Product != null ? r.Product.ProductName : null
                })
                .ToListAsync(cancellationToken);

            return _recentReviewsCache;
        }

        public async Task<StatisticsOverviewDto> GetStatisticsOverviewAsync(
            CancellationToken cancellationToken = default)
        {
            if (_statisticsCache is not null)
            {
                return _statisticsCache;
            }

            var today = _businessClock.Today;
            var weekStart = StartOfWeekMonday(today);
            var weekEndExclusive = weekStart.AddDays(7);
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var thirtyDaysAgo = today.AddDays(-30);

            var reservations = _context.Reservations.AsNoTracking();
            var reviews = _context.Reviews.AsNoTracking();
            var products = _context.Products.AsNoTracking();
            var categories = _context.Categories.AsNoTracking();

            var statusGroups = await reservations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int StatusCount(ReservationStatus status) =>
                statusGroups.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            var pending = StatusCount(ReservationStatus.Pending);
            var confirmed = StatusCount(ReservationStatus.Confirmed);
            var cancelled = StatusCount(ReservationStatus.Cancelled);
            var totalReservations = pending + confirmed + cancelled;

            var totalGuests = await reservations.SumAsync(r => (int?)r.GuestCount, cancellationToken) ?? 0;
            var thisWeekConfirmedGuests = await reservations
                .Where(r => r.ReservationDate >= weekStart
                            && r.ReservationDate < weekEndExclusive
                            && r.Status == ReservationStatus.Confirmed)
                .SumAsync(r => (int?)r.GuestCount, cancellationToken) ?? 0;

            var todayCount = await reservations.CountAsync(
                r => r.ReservationDate == today, cancellationToken);
            var thisWeekCount = await reservations.CountAsync(
                r => r.ReservationDate >= weekStart && r.ReservationDate < weekEndExclusive,
                cancellationToken);
            var thisMonthCount = await reservations.CountAsync(
                r => r.ReservationDate >= monthStart && r.ReservationDate <= today,
                cancellationToken);

            var last30Count = await reservations.CountAsync(
                r => r.ReservationDate >= thirtyDaysAgo && r.ReservationDate <= today,
                cancellationToken);

            var reviewVisibility = await reviews
                .GroupBy(r => r.Status)
                .Select(g => new { Published = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var published = reviewVisibility.FirstOrDefault(x => x.Published)?.Count ?? 0;
            var hidden = reviewVisibility.FirstOrDefault(x => !x.Published)?.Count ?? 0;
            var totalReviews = published + hidden;

            var starCounts = await reviews
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count(), Sum = g.Sum(r => r.Rating) })
                .ToListAsync(cancellationToken);

            int Star(int rating) => starCounts.FirstOrDefault(x => x.Rating == rating)?.Count ?? 0;
            var star5 = Star(5);
            var star4 = Star(4);
            var star3 = Star(3);
            var star2 = Star(2);
            var star1 = Star(1);
            var ratingSum = starCounts.Sum(x => x.Sum);
            var averageRating = totalReviews == 0
                ? 0d
                : Math.Round(ratingSum / (double)totalReviews, 1);

            var activeProducts = await products.CountAsync(p => p.Status, cancellationToken);
            var categoryCounts = await categories
                .GroupBy(c => c.CategoryStatus)
                .Select(g => new { Active = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var activeCategories = categoryCounts.FirstOrDefault(x => x.Active)?.Count ?? 0;
            var totalCategories = categoryCounts.Sum(x => x.Count);

            var categoryRowsRaw = await categories
                .Where(c => c.CategoryStatus)
                .Select(c => new
                {
                    c.CategoryName,
                    ProductCount = c.Products.Count(p => p.Status),
                    AvgPrice = c.Products.Where(p => p.Status).Select(p => (double?)p.Price).Average()
                })
                .OrderByDescending(c => c.ProductCount)
                .Take(5)
                .ToListAsync(cancellationToken);

            var categoryRows = categoryRowsRaw
                .Select(c => new
                {
                    c.CategoryName,
                    c.ProductCount,
                    AvgPrice = c.AvgPrice.HasValue ? Math.Round(c.AvgPrice.Value, 0) : 0d
                })
                .ToList();

            var maxProductCount = categoryRows.Count == 0 ? 1 : Math.Max(1, categoryRows.Max(c => c.ProductCount));
            var topCategories = categoryRows
                .Select(c => new CategoryAnalyticsDto
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.ProductCount,
                    AvgPrice = c.AvgPrice,
                    RelativePercent = (int)Math.Round(c.ProductCount / (double)maxProductCount * 100)
                })
                .ToList();

            var topProductsRaw = await reviews
                .GroupBy(r => new { r.ProductId, r.Product.ProductName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => (double)r.Rating)
                })
                .OrderByDescending(x => x.ReviewCount)
                .ThenByDescending(x => x.AverageRating)
                .Take(5)
                .ToListAsync(cancellationToken);

            var topProducts = topProductsRaw
                .Select(x => new ProductAnalyticsDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ReviewCount = x.ReviewCount,
                    AverageRating = Math.Round(x.AverageRating, 1)
                })
                .ToList();

            var heatmap = await BuildHeatmapAsync(cancellationToken);

            _statisticsCache = new StatisticsOverviewDto
            {
                TotalReservations = totalReservations,
                PendingReservations = pending,
                ConfirmedReservations = confirmed,
                CancelledReservations = cancelled,
                ConfirmationRatePercent = Rate(confirmed, totalReservations),
                CancelRatePercent = Rate(cancelled, totalReservations),
                TotalGuestCount = totalGuests,
                ThisWeekConfirmedGuestCount = thisWeekConfirmedGuests,
                TodayReservations = todayCount,
                ThisWeekReservations = thisWeekCount,
                ThisMonthReservations = thisMonthCount,
                AverageGroupSize = totalReservations == 0
                    ? 0d
                    : Math.Round(totalGuests / (double)totalReservations, 1),
                DailyAverageReservationsLast30Days = Math.Round(last30Count / 30d, 1),
                TotalReviews = totalReviews,
                PublishedReviews = published,
                HiddenReviews = hidden,
                AverageRating = averageRating,
                FiveStarReviews = star5,
                FiveStarPercent = Rate(star5, totalReviews),
                ActiveProducts = activeProducts,
                TotalCategories = totalCategories,
                ActiveCategories = activeCategories,
                StatusSummary = new ReservationStatusSummaryDto
                {
                    Total = totalReservations,
                    Confirmed = confirmed,
                    Pending = pending,
                    Cancelled = cancelled,
                    ConfirmedRatePercent = Rate(confirmed, totalReservations),
                    PendingRatePercent = Rate(pending, totalReservations),
                    CancelledRatePercent = Rate(cancelled, totalReservations)
                },
                TopCategories = topCategories,
                RatingDistribution = new RatingDistributionDto
                {
                    TotalReviews = totalReviews,
                    AverageRating = averageRating,
                    Star5 = star5,
                    Star4 = star4,
                    Star3 = star3,
                    Star2 = star2,
                    Star1 = star1,
                    Pct5 = Rate(star5, totalReviews),
                    Pct4 = Rate(star4, totalReviews),
                    Pct3 = Rate(star3, totalReviews),
                    Pct2 = Rate(star2, totalReviews),
                    Pct1 = Rate(star1, totalReviews)
                },
                TopProductsByReviewCount = topProducts,
                Heatmap = heatmap
            };

            return _statisticsCache;
        }

        private async Task<ReservationHeatmapDto> BuildHeatmapAsync(CancellationToken cancellationToken)
        {
            var slots = await _context.Reservations
                .AsNoTracking()
                .Select(r => new { r.ReservationDate, r.ReservationTime })
                .ToListAsync(cancellationToken);

            var cells = new int[6, 7];
            foreach (var slot in slots)
            {
                var hourIndex = ResolveHourIndex(slot.ReservationTime.Hour);
                if (hourIndex < 0)
                {
                    continue;
                }

                var dayIndex = slot.ReservationDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => 0,
                    DayOfWeek.Tuesday => 1,
                    DayOfWeek.Wednesday => 2,
                    DayOfWeek.Thursday => 3,
                    DayOfWeek.Friday => 4,
                    DayOfWeek.Saturday => 5,
                    DayOfWeek.Sunday => 6,
                    _ => -1
                };
                if (dayIndex < 0)
                {
                    continue;
                }

                cells[hourIndex, dayIndex]++;
            }

            var max = 1;
            var peakH = -1;
            var peakD = -1;
            for (var h = 0; h < 6; h++)
            {
                for (var d = 0; d < 7; d++)
                {
                    if (cells[h, d] > max)
                    {
                        max = cells[h, d];
                        peakH = h;
                        peakD = d;
                    }
                    else if (cells[h, d] == max && cells[h, d] > 0 && peakH < 0)
                    {
                        peakH = h;
                        peakD = d;
                    }
                }
            }

            // If all zeros, peak stays —
            if (max <= 1 && peakH < 0)
            {
                // find any positive or leave —
                for (var h = 0; h < 6 && peakH < 0; h++)
                {
                    for (var d = 0; d < 7; d++)
                    {
                        if (cells[h, d] > 0)
                        {
                            peakH = h;
                            peakD = d;
                            max = Math.Max(max, cells[h, d]);
                            break;
                        }
                    }
                }
            }

            return new ReservationHeatmapDto
            {
                Cells = cells,
                MaxValue = Math.Max(1, max),
                PeakDayLabel = peakD >= 0 ? DayLabelsTr[peakD] : "—",
                PeakHourLabel = peakH >= 0 ? $"{HourSlots[peakH]:00}:00" : "—"
            };
        }

        private static int ResolveHourIndex(int hour)
        {
            for (var i = 0; i < HourSlots.Length; i++)
            {
                if (hour >= HourSlots[i] && (i == HourSlots.Length - 1 || hour < HourSlots[i + 1]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static DateOnly StartOfWeekMonday(DateOnly today)
        {
            var offset = ((int)today.DayOfWeek + 6) % 7;
            return today.AddDays(-offset);
        }

        private static double Rate(int part, int total) =>
            total > 0 ? Math.Round(part / (double)total * 100d, 1) : 0d;
    }
}
