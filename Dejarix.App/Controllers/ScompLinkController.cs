using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.App.Controllers
{
    [Route("scomp-link")]
    [ApiController]
    [Produces("application/json")]
    public class ScompLinkController : ControllerBase
    {
        private readonly DejarixDbContext _context;

        public ScompLinkController(DejarixDbContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
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

        [HttpGet("all-cards")]
        public async Task<IActionResult> AllCards()
        {
            var jsonText = await _context.CardImages
                .Select(ci => ci.InfoJson)
                .ToListAsync(HttpContext.RequestAborted);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var objects = jsonText.ConvertAll(item => JsonSerializer.Deserialize<object>(item));
            return new JsonResult(objects, options);
        }
    }
}