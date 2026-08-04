using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ChartDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ChartServices
{
    public interface IChartService
    {
        Task<List<ReservationChartDto>> GetLast7DaysReservationCountAsync();
        Task<List<CategoryProductCountChartDto>> GetCategoryProductCountAsync();
        Task<List<CategoryAvgPriceChartDto>> GetCategoryAvgPriceAsync();
    }
}
