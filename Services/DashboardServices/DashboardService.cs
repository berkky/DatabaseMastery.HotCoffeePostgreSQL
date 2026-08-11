using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using DatabaseMastery.HotCoffeePostgreSQL.Services;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalReservationCountAsync()
        {
            return await _context.Reservations.CountAsync();
        }

        public async Task<int> GetPendingReservationCountAsync()
        {
            return await _context.Reservations.CountAsync(x =>
                x.Status == "Beklemede" ||
                x.Status == "Pending");
        }

        public async Task<int> GetApprovedReservationCountAsync()
        {
            return await _context.Reservations.CountAsync(x =>
                x.Status == "Onaylandı" ||
                x.Status == "Confirmed");
        }

        public async Task<int> GetCancelledReservationCountAsync()
        {
            return await _context.Reservations.CountAsync(x =>
                x.Status == "İptal Edildi" ||
                x.Status == "Cancelled");
        }

        public async Task<int> GetTodayReservationCountAsync()
        {
            var start = NpgsqlDateTimeCompatibility.AsUtcCalendarDate(DateTime.Today);
            var end = start.AddDays(1);
            return await _context.Reservations.CountAsync(x =>
                x.ReservationDate >= start && x.ReservationDate < end);
        }

        public async Task<int> GetTotalCustomerCountAsync()
        {
            return await _context.Reservations.SumAsync(x => x.GuestCount);
        }

        public async Task<int> GetTotalMenuProductCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public Task<int> GetTodayOrderCountAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ResultReservationDto>> GetTodayReservationListAsync()
        {
            var start = NpgsqlDateTimeCompatibility.AsUtcCalendarDate(DateTime.UtcNow);
            var end = start.AddDays(1);

            var values = await _context.Reservations
                .AsNoTracking()
                .Where(x => x.ReservationDate >= start && x.ReservationDate < end)
                .OrderBy(x => x.ReservationTime)
                .ToListAsync();

            return EntityMappers.ToResultReservationDtos(values);
        }
    }
}
