using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dejarix.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Dejarix.Server.Controllers
{
    public class ScompLinkController : Controller
    {
        public IActionResult Echo()
        {
            var headers = new Dictionary<string, string>();

            foreach (var pair in HttpContext.Request.Headers)
                headers.Add(pair.Key, pair.Value.ToString());
            
            var request = HttpContext.Request;
            return Json(new
            {
                Host = request.Host.ToString(),
                Path = request.Path.ToString(),
                PathBase = request.PathBase.ToString(),
                QueryString = request.QueryString.ToString(),
                Headers = headers
            });
        }

        // [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ServerStatus()
        {
            // if (!User.Identity.IsAuthenticated)
            //     return Unauthorized();

            using (var process = Process.GetCurrentProcess())
            {
                // process.Refresh();
                return Json(new
                {
                    DejarixVersion = GetType().Assembly.GetName().Version.ToString(),
                    UtcStartupTime = Startup.UtcStartupTime.ToString("s"),
                    UtcNow = DateTime.UtcNow.ToString("s"),
                    GcMemory = GC.GetTotalMemory(false),
                    PrivateMemorySize64 = process.PrivateMemorySize64,
                    VirtualMemorySize64 = process.VirtualMemorySize64,
                    WorkingSet64 = process.WorkingSet64,
                    Guess = process.WorkingSet64 / 2
                });
            }
        }

        public async Task<IActionResult> Cards(string title)
        {
            var titleNormalized = title.NormalizedForSearch();

            if (string.IsNullOrEmpty(titleNormalized))
                return Json(new CardImage[]{});

            using (var context = this.GetDbContext())
            {
                var result = await context.CardImages
                    .AsNoTracking()
                    .Where(ci => ci.TitleNormalized.Contains(titleNormalized))
                    .ToListAsync();
                return Json(result);
            }
        }

        public async Task<IActionResult> AllCards()
        {
            using (var context = this.GetDbContext())
            {
                var jsonText = await context.CardImages
                    .AsNoTracking()
                    .Select(ci => ci.InfoJson)
                    .ToListAsync();
                
                var objects = new JArray(jsonText.Select(jt => JObject.Parse(jt)));
                return Json(objects);
            }
        }
    }
}