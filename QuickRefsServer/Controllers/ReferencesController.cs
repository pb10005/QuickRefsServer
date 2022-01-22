using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using QuickRefsServer.Models;

namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferencesController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;
        private readonly IDistributedCache _cache;

        public ReferencesController(QuickRefsDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/References
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reference>>> GetReferences()
        {
            return await _context.References.ToListAsync();
        }

        // GET: api/References
        [HttpGet("findbyknowledge/{id}")]
        public async Task<ActionResult<IEnumerable<Reference>>> findReferencesByKnowledgeId(Guid id)
        {
            return await _context.References.Where(x => x.KnowledgeId == id).ToListAsync();
        }

        // GET: api/References/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reference>> GetReference(Guid id)
        {
            var reference = await _context.References.FindAsync(id);

            if (reference == null)
            {
                return NotFound();
            }

            return reference;
        }

        // PUT: api/References/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReference(Guid id, Reference reference)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var userId = _cache.GetString(sessionId);

            var r = await _context.References.FindAsync(id);
            var k = await _context.Knowledges.FindAsync(r.KnowledgeId);
            var isPrivileged = _context.UserKnowledges.Where(x => x.KnowledgeId == k.Id)
                .Any(x => x.UserId == Guid.Parse(userId));
            if(!isPrivileged)
            {
                return BadRequest("編集権限がありません");
            }

            r.Name = reference.Name;
            r.Description = reference.Description;
            r.Url = reference.Url;
            r.UpdatedAt = DateTime.Now.ToUniversalTime();
            _context.Entry(r).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReferenceExists(id))
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

        // POST: api/References
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Reference>> PostReference(Reference reference)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var userId = _cache.GetString(sessionId);

            var k = await _context.Knowledges.FindAsync(reference.KnowledgeId);
            var isPrivileged = _context.UserKnowledges.Where(x => x.KnowledgeId == k.Id)
                .Any(x => x.UserId == Guid.Parse(userId));

            if(!isPrivileged)
            {
                return BadRequest("追加権限がありません");
            }

            reference.CreatedAt = DateTime.Now.ToUniversalTime();
            reference.UpdatedAt = DateTime.Now.ToUniversalTime();   
            _context.References.Add(reference);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReference", new { id = reference.Id }, reference);
        }

        // DELETE: api/References/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReference(Guid id)
        {
            var reference = await _context.References.FindAsync(id);
            if (reference == null)
            {
                return NotFound();
            }

            _context.References.Remove(reference);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReferenceExists(Guid id)
        {
            return _context.References.Any(e => e.Id == id);
        }
    }
}
