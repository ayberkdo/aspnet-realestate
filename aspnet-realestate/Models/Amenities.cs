namespace aspnet_realestate.Models
{
    public class Amenities : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int AmenitiesGroupId { get; set; }
        public AmenitiesGroup? AmenitiesGroup { get; set; }
    }
}
