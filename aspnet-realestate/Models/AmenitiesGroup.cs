namespace aspnet_realestate.Models
{
    public class AmenitiesGroup : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<Amenities>? Amenities { get; set; }
    }
}
