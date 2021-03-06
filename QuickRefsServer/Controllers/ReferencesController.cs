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
        /*
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reference>>> GetReferences()
        {
            return await _context.References.ToListAsync();
        }
        */

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

            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, reference.KnowledgeId, sessionId);

            return accessibility switch
            {
                KnowledgeAccessibility.None => BadRequest("閲覧権限がありません"),
                KnowledgeAccessibility.Read or KnowledgeAccessibility.ReadAndWrite => reference
            };
        }

        // PUT: api/References/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReference(Guid id, Reference reference)
        {
            var r = await _context.References.FindAsync(id);
            if(r == null)
            {
                return BadRequest("更新対象のデータがありません");
            }

            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, r.KnowledgeId, sessionId);

            if (accessibility == KnowledgeAccessibility.None || accessibility == KnowledgeAccessibility.Read)
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
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, reference.KnowledgeId, sessionId);
           
            if(accessibility == KnowledgeAccessibility.None || accessibility == KnowledgeAccessibility.Read)
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

            Request.Headers.TryGetValue("sessionId", out var sessionId);
            var accessibility = await SessionUtility.CheckKnowledgeAccesibility(_context, _cache, reference.KnowledgeId, sessionId);
            if(accessibility != KnowledgeAccessibility.ReadAndWrite)
            {
                return BadRequest("削除権限がありません");
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
