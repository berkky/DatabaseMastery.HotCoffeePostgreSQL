using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Services;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices
{
    public interface IProductService
    {
        Task<List<ResultProductDto>> GetAllProductsAsync();
        Task<GetProductByIdDto> GetProductByIdAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task CreateProductAsync(CreateProductDto createProductDto);
        Task<bool> UpdateProductAsync(UpdateProductDto updateProductDto);
        Task<DeleteOperationResult> DeleteProductAsync(int id);
    }
}
