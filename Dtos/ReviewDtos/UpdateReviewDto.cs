using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos
{
    public class UpdateReviewDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz yorum kimliği.")]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(ValidationLimits.CustomerNameMax, ErrorMessage = "Müşteri adı çok uzun.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Yorum zorunludur.")]
        [StringLength(ValidationLimits.CommentMax, ErrorMessage = "Yorum çok uzun.")]
        public string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ürün seçin.")]
        public int ProductId { get; set; }
    }
}
