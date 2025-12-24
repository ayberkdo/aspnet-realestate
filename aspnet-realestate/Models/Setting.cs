namespace aspnet_realestate.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string? SiteTitle { get; set; }
        public string? SiteDescription { get; set; }
        public string? SiteKeywords { get; set; }
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? SitePhoneNumber { get; set; }
        public string? SiteEmail { get; set; }
        public string? SiteAddress { get; set; }
        public string? MapEmbedCode { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public DateTime Updated { get; set; } = new DateTime(2025, 11, 1);
    }
}
