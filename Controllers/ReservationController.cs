using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly AppDbContext _context;

        public ReservationController(IReservationService reservationService, AppDbContext context)
        {
            _reservationService = reservationService;
            _context = context;
        }

        public async Task<IActionResult> ReservationList()
        {
            var values = await _reservationService.GetAllReservationsAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateReservation()
        {
            var model = new CreateReservationDto
            {
                GuestCount = 2,
                Status = "Beklemede"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                dto.Status = "Beklemede";
            }

            dto.ReservationDate = DateTime.SpecifyKind(
                dto.ReservationDate.Date,
                DateTimeKind.Utc);

            await _reservationService.CreateReservationAsync(dto);

            return RedirectToAction(nameof(ReservationList));
        }

        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _reservationService.DeleteReservationAsync(id);
            return RedirectToAction(nameof(ReservationList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservation(int id)
        {
            var value = await _reservationService.GetReservationByIdAsync(id);
            if (value == null)
            {
                return RedirectToAction(nameof(ReservationList));
            }

            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservation(UpdateReservationDto updateReservationDto)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new GetReservationByIdDto
                {
                    ReservationId = updateReservationDto.ReservationId,
                    Name = updateReservationDto.Name,
                    Phone = updateReservationDto.Phone,
                    Email = updateReservationDto.Email,
                    ReservationDate = updateReservationDto.ReservationDate,
                    ReservationTime = updateReservationDto.ReservationTime,
                    GuestCount = updateReservationDto.GuestCount,
                    Status = updateReservationDto.Status,
                    Description = updateReservationDto.Description
                };
                return View(viewModel);
            }

            updateReservationDto.ReservationDate = DateTime.SpecifyKind(
                updateReservationDto.ReservationDate.Date,
                DateTimeKind.Utc);

            await _reservationService.UpdateReservationAsync(updateReservationDto);

            return RedirectToAction(nameof(ReservationList));
        }

        public async Task<IActionResult> ApproveReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return RedirectToAction(nameof(ReservationList));
            }

            reservation.Status = "Onaylandı";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReservationList));
        }

        public async Task<IActionResult> PendingReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return RedirectToAction(nameof(ReservationList));
            }

            reservation.Status = "Beklemede";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReservationList));
        }

        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return RedirectToAction(nameof(ReservationList));
            }

            reservation.Status = "İptal Edildi";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReservationList));
        }
    }
}
