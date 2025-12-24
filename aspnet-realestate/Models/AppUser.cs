using Microsoft.AspNetCore.Identity;

namespace aspnet_realestate.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }

        // BaseModel'den gelen özellikler
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;

        // İlişkiler
        public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    }
}
