namespace aspnet_realestate.ViewModels
{
    public class MessagesViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }

        public List<MessageItemViewModel> Messages { get; set; } = new();
    }

    public class MessageItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public bool IsRead { get; set; }
    }

    public class CreateMessageViewModel
    {
        public string Slug { get; set; }
        public string Content { get; set; }
    }
}
