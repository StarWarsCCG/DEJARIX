using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Dejarix.App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Dejarix.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")] public IActionResult Index() => View();
        [HttpGet("privacy")] public IActionResult Privacy() => View();
        [HttpGet("deck-builder")] public IActionResult DeckBuilder() => View();

        [HttpGet("card/{id}")]
        public async Task<IActionResult> Card(
            Guid id,
            [FromServices] DejarixDbContext context)
        {
            var card = await context.CardImages.FindAsync(id);

            if (card is null)
            {
                return NotFound();
            }
            else if (!card.IsFront)
            {
                return RedirectToAction(
                    nameof(Card),
                    ControllerContext.ActionDescriptor.ControllerName,
                    new { id = card.OtherId });
            }
            else
            {
                var model = new CardViewModel
                {
                    Title = card.Title,
                    IsHorizontal = card.IsHorizontal,
                    FrontImage = Url.Content($"~/images/cards/png-370x512/{card.ImageId}.png"),
                    BackImage = Url.Content($"~/images/cards/png-370x512/{card.OtherId}.png")
                };

                return View(model);
            }
        }
        
        [HttpGet("status-code/{statusCode}")]
        public IActionResult StatusCodeGet(int statusCode)
        {
            if (400 <= statusCode && statusCode < 600)
            {
                var model = new StatusCodeViewModel
                {
                    StatusCode = statusCode,
                    ReasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode)
                };

                HttpContext.Response.StatusCode = statusCode;
                return View("StatusCode", model);
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("card-inventory")]
        [Authorize]
        public IActionResult CardInventory() => View("CardInventory");
    }
}