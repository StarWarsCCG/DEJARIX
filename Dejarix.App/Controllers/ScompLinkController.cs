using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<DejarixUser> _userManager;

        public ScompLinkController(
            DejarixDbContext context,
            UserManager<DejarixUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        [HttpGet("exceptions")]
        public async Task<IActionResult> Exceptions()
        {
            var exceptionLogs =
                await _context.ExceptionLogs.ToListAsync(HttpContext.RequestAborted);
            
            var result = exceptionLogs.ConvertAll(log => new
            {
                ExceptionId = log.ExceptionId,
                Ordinal = log.Ordinal,
                Date = log.ExceptionDate,
                Type = log.ExceptionType,
                Message = log.ExceptionMessage,
                StackTrace = log.ExceptionStackTrace?.Split('\n')
            });
            
            return Ok(result);
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
        public async Task<IActionResult> AllCards(CancellationToken cancellationToken)
        {
            var jsonText = await _context.CardImages
                .Select(ci => ci.InfoJson)
                .ToListAsync(cancellationToken);

            var objects = jsonText.ConvertAll(
                item => JsonSerializer.Deserialize<Dictionary<string, object>>(item));
            return Ok(objects);
        }

        [HttpGet("deck-revision/{deckRevisionId}")]
        public async Task<IActionResult> DeckRevision(Guid deckRevisionId)
        {
            var deckRevision = await _context.DeckRevisions
                .Include(dr => dr.Cards)
                .SingleOrDefaultAsync(dr => dr.DeckRevisionId == deckRevisionId);

            if (deckRevision is null)
            {
                return NotFound();
            }
            else
            {
                var cards = deckRevision.Cards;
                var result = new
                {
                    Inside = cards.Where(c => c.InsideCount > 0).ToDictionary(c => c.CardId, c => c.InsideCount),
                    Outside = cards.Where(c => c.OutsideCount > 0).ToDictionary(c => c.CardId, c => c.OutsideCount)
                };

                return Ok(result);
            }
        }

        [HttpGet("deck/{deckId}")]
        public async Task<IActionResult> Deck(Guid deckId)
        {
            var deck = await _context.Decks.FindAsync(deckId);

            if (deck is null)
            {
                return NotFound();
            }
            else
            {
                return await DeckRevision(deck.RevisionId);
            }
        }

        [HttpGet("card-inventory")]
        [Authorize]
        public async Task<IActionResult> CardInventory(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            var cards = await _context.CardInventories
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync(cancellationToken);
            
            var result = cards.ToDictionary(
                c => c.CardImageId.ToString(),
                c => new
                {
                    PublicNotes = c.PublicNotes,
                    PublicHaveCount = c.PublicHaveCount,
                    PublicWantCount = c.PublicWantCount,
                    PrivateNotes = c.PrivateNotes,
                    PrivateHaveCount = c.PrivateHaveCount,
                    PrivateWantCount = c.PrivateWantCount
                });

            return Ok(result);
        }
    }
}