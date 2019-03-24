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
        private readonly IServiceProvider _serviceProvider;

        public ScompLinkController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ServerStatus()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();
                
            return Json(new
            {
                UtcStartupTime = Startup.UtcStartupTime.ToString("s"),
                UtcNow = DateTime.UtcNow.ToString("s"),
                GcMemory = GC.GetTotalMemory(false),
                WorkingSet = Process.GetCurrentProcess().WorkingSet64
            });
        }

        public async Task<IActionResult> Cards(string title)
        {
            var titleNormalized = title.SearchNormalized();

            if (string.IsNullOrEmpty(titleNormalized))
                return Json(new CardImage[]{});

            using (var context = _serviceProvider.GetService<DejarixDbContext>())
            {
                var result = await context.CardImages.Where(cf => cf.TitleNormalized.Contains(titleNormalized)).ToListAsync();
                return Json(result);
            }
        }

        public async Task<IActionResult> AllCards()
        {
            using (var context = _serviceProvider.GetService<DejarixDbContext>())
            {
                var jsonText = await context.CardImages.Select(ci => ci.InfoJson).ToListAsync();
                var objects = new JArray(jsonText.Select(jt => JObject.Parse(jt)));
                return Json(objects);
            }
        }
    }
}