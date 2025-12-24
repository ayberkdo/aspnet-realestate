using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        [Display(Name = "Site adı")]
        public string? SiteTitle { get; set; }


        [Display(Name = "Site Açıklaması")]
        public string? SiteDescription { get; set; }


        [Display(Name = "Site Anahtar Kelimeleri")]
        public string? SiteKeywords { get; set; }


        [Display(Name = "Site Logo URL")]
        public string? LogoUrl { get; set; }


        [Display(Name = "Site Favicon URL")]
        public string? FaviconUrl { get; set; }


        [Display(Name = "Site telefon numarası")]
        public string? SitePhoneNumber { get; set; }


        [Display(Name = "Site email adresi")]
        public string? SiteEmail { get; set; }


        [Display(Name = "Site addressi")]
        public string? SiteAddress { get; set; }


        [Display(Name = "Site harita embed kodu")]
        public string? MapEmbedCode { get; set; }


        [Display(Name = "Instagram")]
        public string? InstagramUrl { get; set; }


        [Display(Name = "Facebook")]
        public string? FacebookUrl { get; set; }


        [Display(Name = "Twitter")]
        public string? TwitterUrl { get; set; }


        [Display(Name = "Youtube")]
        public string? YoutubeUrl { get; set; }
    }
}
