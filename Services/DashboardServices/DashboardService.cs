using AutoMapper;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DashboardService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            return await _context.Reservations.CountAsync(x =>
                x.ReservationDate.Date == DateTime.Today);
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
            var today = DateTime.UtcNow.Date;

            var values = await _context.Reservations
                .Where(x => x.ReservationDate.Date == today)
                .OrderBy(x => x.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ResultReservationDto>>(values);
        }
    }
}
