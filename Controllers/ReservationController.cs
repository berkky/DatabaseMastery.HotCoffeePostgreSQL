using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseMastery.HotCoffeePostgreSQL.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class ReservationController : Controller
    {
        private const string DefaultPendingStatus = "Beklemede";

        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public async Task<IActionResult> ReservationList()
        {
            var values = await _reservationService.GetAllReservationsAsync();
            return View(values);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CreateReservation()
        {
            var model = new CreateReservationDto
            {
                GuestCount = 2
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationDto dto)
        {
            BusinessRequestNormalizer.TrimCreateReservation(dto);
            dto.Status = DefaultPendingStatus;

            ModelState.Clear();
            if (!TryValidateModel(dto))
            {
                return View(dto);
            }

            ReservationRequestRules.ValidateDateNotInPast(
                ModelState,
                dto.ReservationDate,
                nameof(dto.ReservationDate));

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            dto.ReservationDate = DateTime.SpecifyKind(
                dto.ReservationDate.Date,
                DateTimeKind.Utc);

            await _reservationService.CreateReservationAsync(dto);

            return RedirectToAction(nameof(ReservationList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var deleted = await _reservationService.DeleteReservationAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(ReservationList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservation(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var value = await _reservationService.GetReservationByIdAsync(id);
            if (value == null)
            {
                return NotFound();
            }

            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservation(
            int id,
            UpdateReservationDto updateReservationDto)
        {
            if (id != updateReservationDto.ReservationId)
            {
                return BadRequest();
            }

            BusinessRequestNormalizer.TrimUpdateReservation(updateReservationDto);
            ModelState.Clear();
            if (!TryValidateModel(updateReservationDto))
            {
                return View(MapToGetReservationByIdDto(updateReservationDto));
            }

            if (!await _reservationService.ReservationExistsAsync(id))
            {
                return NotFound();
            }

            updateReservationDto.ReservationDate = DateTime.SpecifyKind(
                updateReservationDto.ReservationDate.Date,
                DateTimeKind.Utc);

            var updated = await _reservationService.UpdateReservationAsync(updateReservationDto);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(ReservationList));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var changed = await _reservationService.ChangeReservationStatusToApproval(id);
            if (!changed)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(ReservationList));
        }

        [HttpPost]
        public async Task<IActionResult> PendingReservation(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var changed = await _reservationService.ChangeReservationStatusToPending(id);
            if (!changed)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(ReservationList));
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var changed = await _reservationService.ChangeReservationStatusToCancel(id);
            if (!changed)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(ReservationList));
        }

        private static GetReservationByIdDto MapToGetReservationByIdDto(UpdateReservationDto dto)
        {
            return new GetReservationByIdDto
            {
                ReservationId = dto.ReservationId,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                ReservationDate = dto.ReservationDate,
                ReservationTime = dto.ReservationTime,
                GuestCount = dto.GuestCount,
                Status = dto.Status,
                Description = dto.Description ?? string.Empty
            };
        }
    }
}
