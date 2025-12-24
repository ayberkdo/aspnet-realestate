using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

    }
}
