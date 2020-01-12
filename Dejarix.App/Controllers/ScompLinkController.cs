using System;
using Dejarix.App.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.App.Controllers
{
    [Route("scomp-link")]
    [ApiController]
    public class ScompLinkController : ControllerBase
    {
        private readonly DejarixDbContext _context;

        public ScompLinkController(DejarixDbContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        [Produces("application/json")]
        public IActionResult Test()
        {
            return Ok(new int[] {1, 1, 3, 8});
        }

        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new InvalidOperationException("Blowing up as expected.");
        }

        [HttpGet("status")]
        [Produces("application/json")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ServerStatus()
        {
            var result = new
            {
                DejarixVersion = GetType().Assembly.GetName().Version?.ToString(),
                UtcStart = Startup.UtcStart.ToString("s"),
                UtcNow = DateTime.UtcNow.ToString("s"),
                GcMemory = GC.GetTotalMemory(false)
            };

            return Ok(result);
        }
    }
}