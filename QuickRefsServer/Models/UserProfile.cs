namespace QuickRefsServer.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
