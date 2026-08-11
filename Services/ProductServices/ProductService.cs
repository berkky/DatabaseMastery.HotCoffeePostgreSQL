using DatabaseMastery.HotCoffeePostgreSQL.Context;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(x => x.ProductId == id);
        }

        public async Task CreateProductAsync(CreateProductDto createProductDto)
        {
            var value = EntityMappers.ToEntity(createProductDto);
            await _context.Products.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task<DeleteOperationResult> DeleteProductAsync(int id)
        {
            var value = await _context.Products.FindAsync(id);
            if (value == null)
            {
                return DeleteOperationResult.NotFound;
            }

            if (await _context.Reviews.AnyAsync(r => r.ProductId == id))
            {
                return DeleteOperationResult.BlockedByDependencies;
            }

            _context.Products.Remove(value);

            try
            {
                await _context.SaveChangesAsync();
                return DeleteOperationResult.Deleted;
            }
            catch (DbUpdateException ex) when (DatabaseExceptionHelper.IsForeignKeyViolation(ex))
            {
                return DeleteOperationResult.BlockedByDependencies;
            }
        }

        public async Task<List<ResultProductDto>> GetAllProductsAsync()
        {
            var values = await _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .ToListAsync();
            return EntityMappers.ToResultProductDtos(values);
        }

        public async Task<GetProductByIdDto> GetProductByIdAsync(int id)
        {
            var value = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id);
            return value == null ? null! : EntityMappers.ToGetByIdDto(value);
        }

        public async Task<bool> UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(updateProductDto.ProductId);
            if (product == null)
            {
                return false;
            }

            product.ProductName = updateProductDto.ProductName;
            product.Description = updateProductDto.Description;
            product.ImageUrl = updateProductDto.ImageUrl;
            product.Status = updateProductDto.Status;
            product.Price = updateProductDto.Price;
            product.CategoryId = updateProductDto.CategoryId;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
