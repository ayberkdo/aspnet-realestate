namespace aspnet_realestate.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = new DateTime(2025, 11, 1);
        public DateTime Updated { get; set; } = new DateTime(2025, 11, 1);
    }
}
