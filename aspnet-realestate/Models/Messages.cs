using System.ComponentModel.DataAnnotations.Schema;

namespace aspnet_realestate.Models
{
    public class Messages : BaseModel
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public string Content { get; set; }

        public int? RepliedMessageId { get; set; }
        public Messages RepliedMessage { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? Deleted { get; set; } = null;
    }
}
