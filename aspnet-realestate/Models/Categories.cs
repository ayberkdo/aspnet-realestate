namespace aspnet_realestate.Models
{
    public class Categories : BaseModel
    {
        public int ParentCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<CategoryFields>? CategoryFields { get; set; }
    }
}
