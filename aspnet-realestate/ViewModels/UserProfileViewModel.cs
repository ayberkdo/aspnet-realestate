using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class UserProfileViewModel
    {
        // Sadece bilgi amaçlı gösterilecek (Düzenlenemez)
        public string? Username { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "E-Posta")]
        [Required(ErrorMessage = "E-Posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Biyografi")]
        [MaxLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
        public string? Bio { get; set; }
    }
}