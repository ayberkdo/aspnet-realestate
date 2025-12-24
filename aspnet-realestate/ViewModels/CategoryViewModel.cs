using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        [Display(Name = "Alt Kategori Belirle")]
        public int ParentCategoryId { get; set; }

        // parent id yerine parent name göstermek için
        public string? ParentCategoryName { get; set; }

        [Display(Name = "Kategori adı")]
        [Required(ErrorMessage = "Kategori adı alanı boşa bırakılamaz.")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olmalıdır.")]
        [MinLength(2, ErrorMessage = "Kategori adı en az 2 karakter olmalıdır.")]
        public string? Name {  get; set; }

        [Display(Name="Açıklama")]
        [MaxLength(500, ErrorMessage = "Açıklama alanı en fazla 500 karakter olmalıdır.")]
        public string? Description { get; set; }

        [Display(Name="Slug")]
        [MinLength(2, ErrorMessage = "Slug alanı en az 2 karakter olmalıdır")]
        public string? Slug { get; set; }

        [Display(Name = "Resim URL'si")]
        [MaxLength(200, ErrorMessage = "Resim URL'si en fazla 200 karakter olabilir.")]
        public string? ImageUrl { get; set; }

    }
}
