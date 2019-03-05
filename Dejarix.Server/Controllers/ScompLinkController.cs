using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dejarix.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.Server.Controllers
{
    public class ScompLinkController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public ScompLinkController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Cards(string title)
        {
            var titleNormalized = title.SearchNormalized();

            if (string.IsNullOrEmpty(titleNormalized))
                return Json(new CardFace[]{});

            using (var context = _serviceProvider.GetService<DejarixDbContext>())
            {
                var result = await context.CardFaces.Where(cf => cf.TitleNormalized.Contains(titleNormalized)).ToListAsync();
                return Json(result);
            }
        }
    }
}