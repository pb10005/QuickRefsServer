#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRefsServer.Models;

namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnowledgeTagsController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;

        public KnowledgeTagsController(QuickRefsDbContext context)
        {
            _context = context;
        }

        // GET: api/KnowledgeTags
        [HttpGet("findByKnowledge/{id}")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetKnowledgeTagsByKnowledgeId(Guid id)
        {
            return await _context.Tags
                .Where(t => _context.KnowledgeTags
                            .Where(kt => kt.KnowledgeId == id)
                            .Any(kt => kt.TagId == t.Id))
                .ToListAsync();
        }

        // GET: api/KnowledgeTags
        [HttpGet("findByTag/{id}")]
        public async Task<ActionResult<IEnumerable<Knowledge>>> GetKnowledgeTagsByTagId(Guid id)
        {
            return await _context.Knowledges
                .Where(k => _context.KnowledgeTags
                            .Where(kt => kt.TagId == id)
                            .Any(kt => kt.KnowledgeId == k.Id))
                .ToListAsync();
        }

        // GET: api/KnowledgeTags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KnowledgeTag>> GetKnowledgeTag(Guid id)
        {
            var knowledgeTag = await _context.KnowledgeTags.FindAsync(id);

            if (knowledgeTag == null)
            {
                return NotFound();
            }

            return knowledgeTag;
        }

        // PUT: api/KnowledgeTags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKnowledgeTag(Guid id, KnowledgeTag knowledgeTag)
        {
            if (id != knowledgeTag.KnowledgeId)
            {
                return BadRequest();
            }

            _context.Entry(knowledgeTag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KnowledgeTagExists(id))
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

        // POST: api/KnowledgeTags
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<KnowledgeTag>> PostKnowledgeTag(KnowledgeTagProperty ktProp)
        {
            Tag tag = _context.Tags.SingleOrDefault(t => t.Name == ktProp.TagName);

            if (tag == null)
            {
                return BadRequest("タグがありません");
            }
            
            KnowledgeTag kt = new KnowledgeTag();
            kt.KnowledgeId = ktProp.KnowledgeId;
            kt.TagId = tag.Id;
            _context.KnowledgeTags.Add(kt);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
               throw;
            }

            return CreatedAtAction("GetKnowledgeTag", new { id = kt.KnowledgeId }, kt);
        }

        // DELETE: api/KnowledgeTags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKnowledgeTag(Guid id)
        {
            var knowledgeTag = await _context.KnowledgeTags.FindAsync(id);
            if (knowledgeTag == null)
            {
                return NotFound();
            }

            _context.KnowledgeTags.Remove(knowledgeTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KnowledgeTagExists(Guid id)
        {
            return _context.KnowledgeTags.Any(e => e.KnowledgeId == id);
        }
    }
}
