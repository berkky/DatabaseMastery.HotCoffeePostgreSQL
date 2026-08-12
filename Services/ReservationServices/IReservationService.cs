using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices
{
    public interface IReservationService
    {
        Task<List<ResultReservationDto>> GetAllReservationsAsync();
        Task<GetReservationByIdDto?> GetReservationByIdAsync(int id);
        Task<bool> ReservationExistsAsync(int id);
        Task CreateReservationAsync(CreateReservationDto createReservationDto);
        Task<bool> UpdateReservationAsync(UpdateReservationDto updateReservationDto);
        Task<bool> DeleteReservationAsync(int id);
        Task<bool> ChangeReservationStatusToPending(int id);
        Task<bool> ChangeReservationStatusToApproval(int id);
        Task<bool> ChangeReservationStatusToCancel(int id);
        Task<bool> SetReservationStatusAsync(int id, ReservationStatus status);
    }
}
