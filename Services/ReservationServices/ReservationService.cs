using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ReservationExistsAsync(int id)
        {
            return await _context.Reservations.AnyAsync(x => x.ReservationId == id);
        }

        public async Task<List<ResultReservationDto>> GetAllReservationsAsync()
        {
            var values = await _context.Reservations
                .AsNoTracking()
                .OrderBy(x => x.ReservationId)
                .ToListAsync();
            return EntityMappers.ToResultReservationDtos(values);
        }

        public async Task<GetReservationByIdDto?> GetReservationByIdAsync(int id)
        {
            var value = await _context.Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReservationId == id);
            if (value == null)
            {
                return null;
            }

            return EntityMappers.ToGetByIdDto(value);
        }

        public async Task CreateReservationAsync(CreateReservationDto createReservationDto)
        {
            var value = EntityMappers.ToEntity(createReservationDto);
            await _context.Reservations.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateReservationAsync(UpdateReservationDto updateReservationDto)
        {
            var reservation = await _context.Reservations.FindAsync(updateReservationDto.ReservationId);
            if (reservation == null)
            {
                return false;
            }

            reservation.Name = updateReservationDto.Name;
            reservation.Phone = updateReservationDto.Phone;
            reservation.Email = updateReservationDto.Email;
            reservation.ReservationDate = updateReservationDto.ReservationDate;
            reservation.ReservationTime = updateReservationDto.ReservationTime;
            reservation.GuestCount = updateReservationDto.GuestCount;
            reservation.Status = updateReservationDto.Status;
            reservation.Description = updateReservationDto.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            var value = await _context.Reservations.FindAsync(id);
            if (value == null)
            {
                return false;
            }

            _context.Reservations.Remove(value);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeReservationStatusToPending(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return false;
            }

            reservation.Status = "Beklemede";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeReservationStatusToApproval(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return false;
            }

            reservation.Status = "Onaylandı";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeReservationStatusToCancel(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return false;
            }

            reservation.Status = "İptal Edildi";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
