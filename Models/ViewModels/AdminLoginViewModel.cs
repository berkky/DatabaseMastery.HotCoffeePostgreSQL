using System.ComponentModel.DataAnnotations;

namespace DatabaseMastery.HotCoffeePostgreSQL.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [Display(Name = "Kullanıcı adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
