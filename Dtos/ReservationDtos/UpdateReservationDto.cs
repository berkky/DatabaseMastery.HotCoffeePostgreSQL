using System.ComponentModel.DataAnnotations;
using DatabaseMastery.HotCoffeePostgreSQL.Domain;
using DatabaseMastery.HotCoffeePostgreSQL.Validation;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReservationDtos
{
    public class UpdateReservationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz rezervasyon kimliği.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(ValidationLimits.PersonNameMax, ErrorMessage = "Ad çok uzun.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [StringLength(ValidationLimits.PhoneMax, ErrorMessage = "Telefon çok uzun.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [StringLength(ValidationLimits.EmailMax, ErrorMessage = "E-posta çok uzun.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rezervasyon tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateOnly ReservationDate { get; set; }

        [Required(ErrorMessage = "Rezervasyon saati zorunludur.")]
        [DataType(DataType.Time)]
        public TimeOnly ReservationTime { get; set; }

        [Range(1, ValidationLimits.GuestCountMax, ErrorMessage = "Kişi sayısı 1 ile 50 arasında olmalıdır.")]
        public int GuestCount { get; set; }

        [Required(ErrorMessage = "Durum zorunludur.")]
        [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Geçersiz rezervasyon durumu.")]
        public ReservationStatus Status { get; set; }

        [StringLength(ValidationLimits.ReservationDescriptionMax, ErrorMessage = "Not çok uzun.")]
        public string? Description { get; set; }
    }
}
