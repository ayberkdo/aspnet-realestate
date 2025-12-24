using System.ComponentModel.DataAnnotations.Schema;

namespace aspnet_realestate.Models
{
    public class Property : BaseModel
    {
        // 🔹 Kullanıcı (İlan Sahibi)
        public string UserId { get; set; }
        public AppUser? User { get; set; }

        // 🔹 Kategori (ör: Daire, Villa, Arsa)
        public int CategoryId { get; set; }
        public Categories? Category { get; set; }

        // 🔹 Konum / Lokasyon (İlişki)
        public int PropertyDestinationId { get; set; }
        public PropertyDestination PropertyDestination { get; set; } = null!;

        // 🔹 Temel Bilgiler
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }

        // 🔹 Satış Türü (ör: sale, rent)
        public string Type { get; set; } = "sale";

        // 🔹 Fiyat Bilgileri
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "TRY";

        // 🔹 Adres ve Konum Bilgileri
        public string? Address { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? MapLat { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? MapLng { get; set; }

        // 🔹 Kapak Görseli
        public string? CoverImage { get; set; }

        // 🔹 Ek Bilgi Alanı
        public string? Other { get; set; }

        // 🔹 Durumlar
        public string Status { get; set; } = "approval";
        public string? ApprovalUserId { get; set; }

        // 🔹 SEO Alanları
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }

        // 🔹 Yayınlanma Tarihi
        public DateTime? PublishedAt { get; set; }

        // 🔹 İlişkiler
        public ICollection<PropertyImage>? Images { get; set; } = new List<PropertyImage>();
        public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    }
}
