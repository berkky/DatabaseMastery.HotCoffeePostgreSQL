using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ChartDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services.Time;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ChartServices
{
    public class ChartService : IChartService
    {
        private readonly AppDbContext _context;
        private readonly IBusinessClock _businessClock;

        public ChartService(AppDbContext context, IBusinessClock businessClock)
        {
            _context = context;
            _businessClock = businessClock;
        }

        public async Task<List<ReservationChartDto>> GetLast7DaysReservationCountAsync()
        {
            var today = _businessClock.Today;
            var startInclusive = today.AddDays(-6);

            var reservationDates = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.ReservationDate >= startInclusive && r.ReservationDate <= today)
                .Select(r => r.ReservationDate)
                .ToListAsync();

            var grouped = reservationDates
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count());

            return Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var date = startInclusive.AddDays(i);
                    return new ReservationChartDto
                    {
                        Day = TurkishDatePresentation.FormatDateShort(date),
                        Count = grouped.TryGetValue(date, out var count) ? count : 0
                    };
                })
                .ToList();
        }

        public async Task<List<CategoryProductCountChartDto>> GetCategoryProductCountAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryStatus)
                .Select(c => new CategoryProductCountChartDto
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(p => p.Status)
                })
                .ToListAsync();
        }

        public async Task<List<CategoryAvgPriceChartDto>> GetCategoryAvgPriceAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryStatus)
                .Select(c => new CategoryAvgPriceChartDto
                {
                    CategoryName = c.CategoryName,
                    AvgPrice = c.Products
                        .Where(p => p.Status)
                        .Any()
                            ? Math.Round(
                                (decimal)c.Products
                                    .Where(p => p.Status)
                                    .Average(p => (double)p.Price),
                                2)
                            : 0
                })
                .OrderByDescending(c => c.AvgPrice)
                .ToListAsync();
        }
    }
}
