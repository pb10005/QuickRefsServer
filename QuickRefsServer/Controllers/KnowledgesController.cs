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
            return await _context.Knowledges.ToListAsync();
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

            return knowledge;
        }

        // PUT: api/Knowledges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKnowledge(Guid id, Knowledge knowledge)
        {
            if (id != knowledge.Id)
            {
                return BadRequest();
            }

            knowledge.UpdatedAt = DateTime.Now.ToUniversalTime();
            _context.Entry(knowledge).State = EntityState.Modified;

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
