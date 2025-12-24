using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı alanı gereklidir.")]
        public string Username { get; set; }

        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola alanı gereklidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}