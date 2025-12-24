using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class CategoryFieldsViewModel : BaseViewModel
    {
        [Display(Name = "Grup belirle")]
        [Range(1, int.MaxValue, ErrorMessage = "Grup alanı belirleyin.")]
        public int CategoryId { get; set; }

        // id yerine namei göstermek için;
        public string? GroupName { get; set; }

        [Display(Name = "Alan tanımla")]
        public string? FieldName { get; set; }
    }
}
