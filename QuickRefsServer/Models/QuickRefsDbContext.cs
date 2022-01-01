using Microsoft.EntityFrameworkCore;

namespace QuickRefsServer.Models
{
    public class QuickRefsDbContext: DbContext
    {

        public QuickRefsDbContext(DbContextOptions<QuickRefsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Knowledge> Knowledges { get; set; } = null!;
        public DbSet<Reference> References { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserKnowledge> UserKnowledges { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!; 
        public DbSet<KnowledgeTag> KnowledgeTags { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KnowledgeTag>()
                .HasKey(kt => new { kt.KnowledgeId, kt.TagId });
        }
    }
}
