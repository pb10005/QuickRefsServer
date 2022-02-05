using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using QuickRefsServer.Models;
using QuickRefsServer.Utils;

namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserKnowledgesController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;
        private readonly IDistributedCache _cache;

        public UserKnowledgesController(QuickRefsDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/UserKnowledges/findByUser/5
        [HttpGet("findByUser/{userId}")]
        public async Task<ActionResult<IEnumerable<Knowledge>>> GetUserKnowledgeByUser(string userId)
        {
            return await _context.Knowledges
                    .Where(k => _context.UserKnowledges.Where(uk => uk.UserId == Guid.Parse(userId))
                    .Any(uk => uk.KnowledgeId == k.Id))
                    .ToListAsync();
        }

        // GET: api/UserKnowledges/findByKnowledge/5
        [HttpGet("findByKnowledge/{knowledgeId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUserKnowledgeByKnowledge(string knowledgeId)
        {
            return await _context.Users
                    .Where(u => _context.UserKnowledges.Where(uk => uk.KnowledgeId == Guid.Parse(knowledgeId))
                    .Any(uk => uk.UserId == u.Id))
                    .ToListAsync();
        }

        // GET: api/UserKnowledges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserKnowledge>> GetUserKnowledge(Guid id)
        {
            var userKnowledge = await _context.UserKnowledges.FindAsync(id);

            if (userKnowledge == null)
            {
                return NotFound();
            }

            return userKnowledge;
        }

        // PUT: api/UserKnowledges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{toUserName}")]
        public async Task<IActionResult> PutUserKnowledge(string toUserName, UserKnowledge userKnowledge)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var  fromUserId = await _cache.GetStringAsync(sessionId);
            User? toUser = await _context.Users.SingleOrDefaultAsync(u => u.Name == toUserName);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, userKnowledge.KnowledgeId, sessionId);

            if (accessibility != KnowledgeAccessibility.ReadAndWrite)
            {
                return BadRequest("実行権限がありません");
            }
            UserKnowledge? uk = await _context.UserKnowledges
                .SingleOrDefaultAsync(uk => uk.KnowledgeId == userKnowledge.KnowledgeId
                                            && uk.UserId == Guid.Parse(fromUserId));
            
            if(uk == default(UserKnowledge) || toUser == default(User))
            {
                return BadRequest("引数が不正です");
            }

            uk.UserId = toUser.Id;
            _context.Entry(uk).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/UserKnowledges
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserKnowledge>> PostUserKnowledge(UserKnowledge userKnowledge)
        {
            _context.UserKnowledges.Add(userKnowledge);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserKnowledge", new { id = userKnowledge.Id }, userKnowledge);
        }

        // DELETE: api/UserKnowledges/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserKnowledge(Guid id)
        {
            var userKnowledge = await _context.UserKnowledges.FindAsync(id);
            if (userKnowledge == null)
            {
                return NotFound();
            }

            _context.UserKnowledges.Remove(userKnowledge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserKnowledgeExists(Guid id)
        {
            return _context.UserKnowledges.Any(e => e.Id == id);
        }
    }
}
