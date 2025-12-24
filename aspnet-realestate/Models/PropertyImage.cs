namespace aspnet_realestate.Models
{
    public class PropertyImage : BaseModel
    {
        public int PropertyId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int Order { get; set; } = 0;

        public Property Property { get; set; } = null!;
    }
}
