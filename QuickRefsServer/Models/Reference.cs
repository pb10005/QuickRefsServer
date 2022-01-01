namespace QuickRefsServer.Models
{
    public class Reference
    {
        public Guid Id { get; set; }
        public Guid KnowledgeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
