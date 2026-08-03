using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ReservationServices
{
    public interface IReservationService
    {
        Task<List<ResultReservationDto>> GetAllReservationsAsync();
        Task<GetReservationByIdDto?> GetReservationByIdAsync(int id);
        Task CreateReservationAsync(CreateReservationDto createReservationDto);
        Task UpdateReservationAsync(UpdateReservationDto updateReservationDto);
        Task DeleteReservationAsync(int id);
    }
}
