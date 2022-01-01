namespace QuickRefsServer.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public UserProfile ToUserProfile()
        {
            var profile = new UserProfile()
            {
                Id = Id,
                Name = Name,
                ScreenName = ScreenName,
                IsAdmin = IsAdmin,
                IsDeleted = IsDeleted,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
            return profile;
        }
    }
}
