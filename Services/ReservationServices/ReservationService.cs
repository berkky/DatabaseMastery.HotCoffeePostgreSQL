using AutoMapper;
using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReservationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ResultReservationDto>> GetAllReservationsAsync()
        {
            var values = await _context.Reservations
                .OrderBy(x => x.ReservationId)
                .ToListAsync();
            return _mapper.Map<List<ResultReservationDto>>(values);
        }

        public async Task<GetReservationByIdDto?> GetReservationByIdAsync(int id)
        {
            var value = await _context.Reservations.FindAsync(id);
            if (value == null)
            {
                return null;
            }

            return _mapper.Map<GetReservationByIdDto>(value);
        }

        public async Task CreateReservationAsync(CreateReservationDto createReservationDto)
        {
            var value = _mapper.Map<Reservation>(createReservationDto);
            await _context.Reservations.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReservationAsync(UpdateReservationDto updateReservationDto)
        {
            var reservation = await _context.Reservations.FindAsync(updateReservationDto.ReservationId);
            if (reservation == null)
            {
                return;
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
        }

        public async Task DeleteReservationAsync(int id)
        {
            var value = await _context.Reservations.FindAsync(id);
            if (value == null)
            {
                return;
            }

            _context.Reservations.Remove(value);
            await _context.SaveChangesAsync();
        }
    }
}
