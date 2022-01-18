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
    public class UserKnowledgesController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;

        public UserKnowledgesController(QuickRefsDbContext context)
        {
            _context = context;
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserKnowledge(Guid id, UserKnowledge userKnowledge)
        {
            if (id != userKnowledge.Id)
            {
                return BadRequest();
            }

            _context.Entry(userKnowledge).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserKnowledgeExists(id))
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
