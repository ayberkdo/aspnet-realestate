using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.ViewModels
{
    public class PropertyViewModel : BaseViewModel
    {
        
        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Satış türü seçilmelidir.")]
        public string Type { get; set; } = "sale"; // sale / rent

        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Kategoriye özel alanlar (ör: metrekare, oda sayısı)
        // Bunlar dinamik olarak formda oluşturulacak
        public List<PropertyCustomFieldViewModel>? CustomFields { get; set; }

        public int DestinationId { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }

        public string? Address { get; set; }
        public decimal? MapLat { get; set; }
        public decimal? MapLng { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal Price { get; set; }

        public string Currency { get; set; } = "TRY";

        // Eğer tür = rent ise, bu alanlar aktif olur
        public decimal? DailyPrice { get; set; }
        public decimal? WeeklyPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }

        // Bu, kullanıcı tarafından seçilen olanak ID’lerini tutacak
        public List<int>? SelectedAmenities { get; set; }

        // Tüm grupları ve içindeki olanakları listelemek için
        public List<PropertyAmenitiesGroupViewModel>? AmenitiesGroups { get; set; }

        public string? CoverImageUrl { get; set; }

        // !!! Veritabanına CoverImage olarak kaydedildiği için listelemede bu bölümü ekledim
        public string? CoverImage { get; set; }

        // Ek fotoğraflar (PropertyImages tablosuna eklenecek)
        public List<PropertyImageViewModel> Images { get; set; } = new();

        public string Status { get; set; } = "approval";
        public string? ApprovalUserId { get; set; }


        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
    }

    public class PropertyCustomFieldViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string? Value { get; set; }
    }

    public class PropertyAmenitiesGroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PropertyAmenitiesItemViewModel> Amenities { get; set; } = new();
    }

    public class PropertyAmenitiesItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool Selected { get; set; } = false;
    }

    public class PropertyImageViewModel
    {
        public string? ImageUrl { get; set; }
        public string? AltText { get; set; }
    }

    public class PropertyDestinationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ülke seçilmelidir.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir seçilmelidir.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlçe seçilmelidir.")]
        public string District { get; set; } = string.Empty;
    }

}
