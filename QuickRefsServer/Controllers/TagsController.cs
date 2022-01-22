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
    public class TagsController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;
        private readonly IDistributedCache _cache;
        public TagsController(QuickRefsDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            string userId = await _cache.GetStringAsync(sessionId);

            // 閲覧可能なナレッジ：自分のナレッジまたはPublic
            var visibleKnowledgeList = _context.UserKnowledges
                .Where(uk => uk.UserId.ToString() == userId)
                .Select(uk => uk.KnowledgeId)
                .Union(_context.Knowledges.Where(k => !k.IsPrivate).Select(k => k.Id))
                .Distinct();

            // 閲覧可能なナレッジが存在するタグ
            var visibleTags = _context.KnowledgeTags
                .Where(kt => visibleKnowledgeList.Contains(kt.KnowledgeId))
                .Select(kt => kt.TagId);

            return await _context.Tags
                .Where(t => visibleTags.Contains(t.Id))
                .ToListAsync();

        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }
        // GET: api/Tags/5
        [HttpGet("findByName/{name}")]
        public async Task<ActionResult<Tag>> GetTagByName(string name)
        {
            var tag = await _context.Tags.SingleOrDefaultAsync(t => t.Name == name);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }

        // PUT: api/Tags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(Guid id, Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // POST: api/Tags
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(TagProfile tagProfile)
        {
            Request.Headers.TryGetValue("sessionId", out var sessionId);
            if(string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("ログインしてください");
            }

            Tag tag = new Tag();
            tag.Id = Guid.NewGuid();
            tag.Name = tagProfile.Name;
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TagExists(Guid id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
