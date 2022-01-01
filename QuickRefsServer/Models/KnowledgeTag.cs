using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuickRefsServer.Models
{
    public class KnowledgeTag
    {
        public Guid KnowledgeId { get; set; }
        public Guid TagId { get; set; }
    }
}
