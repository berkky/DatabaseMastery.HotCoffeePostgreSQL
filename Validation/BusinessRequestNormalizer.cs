using DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos;
using DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos;

namespace DatabaseMastery.HotCoffeePostgreSQL.Validation
{
    public static class BusinessRequestNormalizer
    {
        public static string TrimToEmpty(string? value) => value?.Trim() ?? string.Empty;

        public static string? TrimOptional(string? value)
        {
            if (value == null)
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        public static void TrimCreateCategory(CreateCategoryDto dto)
        {
            dto.CategoryName = TrimToEmpty(dto.CategoryName);
            dto.CategoryImageUrl = TrimToEmpty(dto.CategoryImageUrl);
        }

        public static void TrimUpdateCategory(UpdateCategoryDto dto)
        {
            dto.CategoryName = TrimToEmpty(dto.CategoryName);
            dto.CategoryImageUrl = TrimToEmpty(dto.CategoryImageUrl);
        }

        public static void TrimCreateProduct(CreateProductDto dto)
        {
            dto.ProductName = TrimToEmpty(dto.ProductName);
            dto.Description = TrimToEmpty(dto.Description);
            dto.ImageUrl = TrimToEmpty(dto.ImageUrl);
        }

        public static void TrimUpdateProduct(UpdateProductDto dto)
        {
            dto.ProductName = TrimToEmpty(dto.ProductName);
            dto.Description = TrimToEmpty(dto.Description);
            dto.ImageUrl = TrimToEmpty(dto.ImageUrl);
        }

        public static void TrimCreateReservation(CreateReservationDto dto)
        {
            dto.Name = TrimToEmpty(dto.Name);
            dto.Phone = TrimToEmpty(dto.Phone);
            dto.Email = TrimToEmpty(dto.Email);
            dto.Description = TrimOptional(dto.Description);
            dto.Status = TrimToEmpty(dto.Status);
        }

        public static void TrimUpdateReservation(UpdateReservationDto dto)
        {
            dto.Name = TrimToEmpty(dto.Name);
            dto.Phone = TrimToEmpty(dto.Phone);
            dto.Email = TrimToEmpty(dto.Email);
            dto.Description = TrimOptional(dto.Description);
            dto.Status = TrimToEmpty(dto.Status);
        }

        public static void TrimCreateReview(CreateReviewDto dto)
        {
            dto.CustomerName = TrimToEmpty(dto.CustomerName);
            dto.Comment = TrimToEmpty(dto.Comment);
        }

        public static void TrimUpdateReview(UpdateReviewDto dto)
        {
            dto.CustomerName = TrimToEmpty(dto.CustomerName);
            dto.Comment = TrimToEmpty(dto.Comment);
        }
    }
}
