using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Dejarix.App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dejarix.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly DejarixDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            DejarixDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")] public IActionResult Index() => View();
        [HttpGet("privacy")] public IActionResult Privacy() => View();
        [HttpGet("deck-builder")] public IActionResult DeckBuilder() => View();
        
        [HttpGet("decks")]
        public async Task<IActionResult> Decks()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        [HttpGet("decks/view/{deckId}")]
        public async Task<IActionResult> ViewDeck(
            Guid deckId,
            [FromServices] UserManager<DejarixUser> userManager,
            CancellationToken cancellationToken)
        {
            var deck = await _context.Decks
                .Include(d => d.Revision)
                .ThenInclude(dr => dr.Cards)
                .ThenInclude(cidr => cidr.Card)
                .SingleOrDefaultAsync(d => d.DeckId == deckId, cancellationToken);
            
            if (deck is null)
            {
                return NotFound();
            }
            else
            {
                var model = new DeckViewModel
                {
                    Deck = deck,
                    PageTitle = deck.Revision?.Title ?? deck.DeckId.ToString()
                };

                if (User.Identity.IsAuthenticated)
                {
                    var user = await userManager.GetUserAsync(User);
                    model.ShowEditLink = user.Id == deck.CreatorId;
                }

                return View(model);
            }
        }

        [HttpGet("decks/create")]
        [Authorize]
        public IActionResult CreateDeck()
        {
            return RedirectToAction(
                nameof(EditDeck),
                new { deckId = Guid.NewGuid() });
        }

        [HttpGet("decks/edit/{deckId}")]
        [Authorize]
        public IActionResult EditDeck(Guid deckId)
        {
            var model = new EditDeckViewModel { DeckId = deckId };
            return View(model);
        }

        [HttpGet("card/{cardId}")]
        public async Task<IActionResult> Card(
            Guid cardId,
            CancellationToken cancellationToken)
        {
            var card = await _context.CardImages
                .SingleOrDefaultAsync(c => c.ImageId == cardId, cancellationToken);

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

        [HttpGet("card-inventory/{userId}")]
        public IActionResult ViewCardInventory()
        {
            throw new NotImplementedException();
        }
    }
}