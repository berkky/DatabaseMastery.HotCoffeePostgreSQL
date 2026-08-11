using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.ViewComponents.StatisticsViewComponents
{
    public class _StatisticsBigGridComponentPartial : ViewComponent
    {
        private readonly AppDbContext _context;
        public _StatisticsBigGridComponentPartial(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            // Ortalama grup büyüklügü (rezervasyon basina kisi)
            var totalReservation = _context.Reservations.Count();
            var totalGuest = _context.Reservations.Sum(r => r.GuestCount);
            ViewBag.avgGroupSize = totalReservation > 0
                ? Math.Round((double)totalGuest / totalReservation, 1)
                : 0;

            // Ortalama müsteri puani
            ViewBag.avgRating = _context.Reviews.Any()
                ? Math.Round(_context.Reviews.Average(r => r.Rating), 1)
                : 0;
            ViewBag.totalReview = _context.Reviews.Count();

            // Günlük ortalama rezervasyon (son 30 gün)
            var thirtyDaysAgo = NpgsqlDateTimeCompatibility.AsUtcCalendarDate(DateTime.UtcNow).AddDays(-30);
            var last30Count = _context.Reservations
                .Count(r => r.ReservationDate >= thirtyDaysAgo);
            ViewBag.dailyAvgReservation = Math.Round((double)last30Count / 30, 1);

            return View();
        }
    }
}
