using System.ComponentModel.DataAnnotations;

namespace aspnet_realestate.Models
{
    public class PropertyDestination : BaseModel
    {
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        public ICollection<Property>? Properties { get; set; }
    }
}
