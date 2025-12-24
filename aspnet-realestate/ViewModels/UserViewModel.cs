using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "İsim & Soyisim")]
        [MaxLength(50, ErrorMessage = "İsim & Soyisim en fazla 50 karakter olabilir.")]
        public string? FullName { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı alanı boş bırakılamaz.")]
        [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
        [MaxLength(24, ErrorMessage = "Kullanıcı adı en fazla 24 karakter olabilir.")]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Kullanıcı adı sadece harfler, rakamlar ve alt çizgi (_) içerebilir.")]
        public string? UserName { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası alanı boş bırakılamaz.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta alanı boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Rol alanı boş bırakılamaz.")]
        public string? Role { get; set; } = "User";

        [Display(Name = "Profil Resmi URL'si")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Biyografi")]
        [MaxLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
        public string? Bio { get; set; }

        // BaseModel'den manuel olarak aldığımız alanlar
        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kayıt Tarihi")]
        public DateTime Created { get; set; }

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime Updated { get; set; }

    }

    public class AddUserViewModel : UserViewModel
    {
        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$", ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.")]
        public string? Password { get; set; }
    }
}
