using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.CategoryDtos
{
    public class UpdateCategoryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz kategori kimliği.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(ValidationLimits.CategoryNameMax, ErrorMessage = "Kategori adı çok uzun.")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Görsel yolu zorunludur.")]
        [StringLength(ValidationLimits.ImagePathMax, ErrorMessage = "Görsel yolu çok uzun.")]
        public string CategoryImageUrl { get; set; }

        public bool CategoryStatus { get; set; }
    }
}
