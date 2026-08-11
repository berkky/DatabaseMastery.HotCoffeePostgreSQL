using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(ValidationLimits.ProductNameMax, ErrorMessage = "Ürün adı çok uzun.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(ValidationLimits.DescriptionMax, ErrorMessage = "Açıklama çok uzun.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Görsel yolu zorunludur.")]
        [StringLength(ValidationLimits.ImagePathMax, ErrorMessage = "Görsel yolu çok uzun.")]
        public string ImageUrl { get; set; }

        public bool Status { get; set; }

        [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Fiyat 0,01 ile 999999,99 arasında olmalıdır.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçin.")]
        public int CategoryId { get; set; }
    }
}
