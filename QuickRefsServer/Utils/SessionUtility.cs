using Microsoft.Extensions.Caching.Distributed;
using QuickRefsServer.Models;

namespace QuickRefsServer.Utils
{
    public enum KnowledgeAccessibility
    {
        None,
        Read,
        ReadAndWrite
    }

    public static class SessionUtility
    {
        public static async Task<KnowledgeAccessibility> CheckKnowledgeAccesibility(QuickRefsDbContext context, IDistributedCache cache, Guid knowledgeId, string sessionId)
        {
            string userId = await cache.GetStringAsync(sessionId);
            var knowledge = await context.Knowledges.FindAsync(knowledgeId);

            if (knowledge == null)
            {
                return KnowledgeAccessibility.None;
            }


            bool isOwner = context.UserKnowledges
                .Where(uk => uk.KnowledgeId == knowledgeId)
                .Any(uk => uk.UserId.ToString() == userId);

            if(isOwner)
            {
                // Owner
                return KnowledgeAccessibility.ReadAndWrite;
            }
            else
            {
                // Not owner
                if (knowledge.IsPrivate)
                {
                    // Private
                    return KnowledgeAccessibility.None;
                }
                else
                {
                    // Public
                    return KnowledgeAccessibility.Read;
                }
            }
        }
    }
}
