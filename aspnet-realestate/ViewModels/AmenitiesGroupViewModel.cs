using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class AmenitiesGroupViewModel : BaseViewModel
    {
        [Display(Name = "Olanak Grup adı")]
        [Required(ErrorMessage = "Olanak grup adı alanı boş bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Olanak grup adı en fazla 100 karakter olabilir.")]
        [MinLength(2, ErrorMessage = "Olanak grup adı en az 2 karakter olmalıdır.")]
        public string Name { get; set; } = "";

        [Display(Name = "Açıklama")]
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [Display(Name = "Resim URL'si")]
        [MaxLength(200, ErrorMessage = "Resim URL'si en fazla 200 karakter olabilir.")]
        public string? ImageUrl { get; set; }
    }
}
