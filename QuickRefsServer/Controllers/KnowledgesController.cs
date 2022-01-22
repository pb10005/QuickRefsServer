using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRefsServer.Models;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using QuickRefsServer.Utils;


namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnowledgesController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;
        private readonly IDistributedCache _cache;

        public KnowledgesController(QuickRefsDbContext context,  IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/Knowledges
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Knowledge>>> GetKnowledges()
        {
            return await _context.Knowledges
                .Where(k => !k.IsPrivate)
                .ToListAsync();
        }

        // GET: api/Knowledges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Knowledge>> GetKnowledge(Guid id)
        {
            var knowledge = await _context.Knowledges.FindAsync(id);

            if (knowledge == null)
            {
                return NotFound();
            }

            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, id, sessionId);
            return accessibility switch
            {
                KnowledgeAccessibility.None => BadRequest("閲覧権限がありません"),
                KnowledgeAccessibility.Read or KnowledgeAccessibility.ReadAndWrite => knowledge,
                _ => throw new NotImplementedException()
            };

        }

        // PUT: api/Knowledges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKnowledge(Guid id, Knowledge knowledge)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, id, sessionId);
            if(accessibility == KnowledgeAccessibility.None || accessibility == KnowledgeAccessibility.Read)
            {
                return BadRequest("更新権限がありません");
            }

            var k = await _context.Knowledges.FindAsync(id);

            if (k == null)
            {
                return BadRequest();
            }

            k.Name = knowledge.Name;
            k.Description = knowledge.Description;
            k.IsPrivate = knowledge.IsPrivate;
            k.UpdatedAt = DateTime.Now.ToUniversalTime();
            _context.Entry(k).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KnowledgeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Knowledges
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Knowledge>> PostKnowledge(Knowledge knowledge)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var id = _cache.GetString(sessionId);
            if (id != null)
            {
                knowledge.CreatedAt = DateTime.Now.ToUniversalTime();
                knowledge.UpdatedAt = DateTime.Now.ToUniversalTime();

                _context.Knowledges.Add(knowledge);

                UserKnowledge un = new UserKnowledge();
                un.Id = Guid.NewGuid();
                un.UserId = Guid.Parse(id);
                un.KnowledgeId = knowledge.Id;
                _context.UserKnowledges.Add(un);

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetKnowledge", new { id = knowledge.Id }, knowledge);
            }
            else
            {
                return BadRequest("だめです ");
            }
        }

        // DELETE: api/Knowledges/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKnowledge(Guid id)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, id, sessionId);

            if(accessibility == KnowledgeAccessibility.None || accessibility == KnowledgeAccessibility.Read)
            {
                return BadRequest("削除権限がありません");
            }

            var knowledge = await _context.Knowledges.FindAsync(id);
            if (knowledge == null)
            {
                return NotFound();
            }

            _context.Knowledges.Remove(knowledge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KnowledgeExists(Guid id)
        {
            return _context.Knowledges.Any(e => e.Id == id);
        }
    }
}
