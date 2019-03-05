using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dejarix.Server.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dejarix.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        

        public async Task<IActionResult> Card(Guid id)
        {
            using (var context = _serviceProvider.GetService<DejarixDbContext>())
            {
                var card = await context.Cards.FindAsync(id);

                if (card == null)
                {
                    return NotFound();
                }
                else
                {
                    var cardFace = await context.CardFaces.FindAsync(card.FrontImageId);
                    ViewData["Title"] = cardFace?.Title ?? "untitled card";
                    ViewData["FrontImage"] = Url.Content($"~/images/cards/{card.FrontImageId}.jpg");
                    ViewData["BackImage"] = Url.Content($"~/images/cards/{card.BackImageId}.jpg");
                    return View();
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
