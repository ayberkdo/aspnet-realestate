using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "İsim ve Soyisim")]
        [Required(ErrorMessage = "İsim ve soyisim alanı gereklidir.")]
        public string FullName { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı alanı gereklidir.")]
        public string Username { get; set; }

        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta alanı gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")] // Aktif edildi
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası alanı gereklidir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")] // Aktif edildi
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola alanı gereklidir.")]
        [DataType(DataType.Password)] // UI'da gizli görünmesi için
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Parola en az 8 karakter uzunluğunda olmalı ve en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string Password { get; set; }

        [Display(Name = "Parolayı Onayla")]
        [Required(ErrorMessage = "Parolayı onaylama alanı gereklidir.")]
        [DataType(DataType.Password)] // UI'da gizli görünmesi için
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}