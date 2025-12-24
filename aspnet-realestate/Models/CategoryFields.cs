namespace aspnet_realestate.Models
{
    public class CategoryFields : BaseModel
    {
        public int CategoryId { get; set; }
        public string? FieldName { get; set; }
        public Categories? Categories { get; set; }
    }
}
