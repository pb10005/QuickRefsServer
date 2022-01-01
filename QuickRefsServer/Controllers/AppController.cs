using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRefsServer.Models;
using System.Linq;

namespace QuickRefsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly QuickRefsDbContext _context;

        public AppController(QuickRefsDbContext context)
        {
            _context = context;
        }

        // GET: api/app
        [HttpGet]
        public ActionResult<string> GetKnowledges()
        {
            return "Success";
        }
    }
}
