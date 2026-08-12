using DatabaseMastery.HotCoffeePostgreSQL.Dtos.PublicRestaurantDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant
{
    public interface IPublicRestaurantService
    {
        Task<PublicLandingDto> GetLandingAsync(CancellationToken cancellationToken = default);
    }
}
