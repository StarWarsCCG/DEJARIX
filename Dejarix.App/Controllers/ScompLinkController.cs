using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
                item => JsonSerializer.Deserialize<JsonElement>(item));
            
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

        [HttpPost("deck-import/holotable")]
        [RequestSizeLimit(100 << 10)] // 100 KiB
        public async Task<IActionResult> PostDeckImportHolotable(CancellationToken cancellationToken)
        {
            var result = new List<CardImage>();
            var fields = new List<string>();

            using (var reader = new StreamReader(Request.Body))
            {
                while (true)
                {
                    var line = await reader.ReadLineAsync();

                    if (line is null)
                        break;
                    
                    fields.Clear();
                    Holotable.AddFields(fields, line);

                    if (fields.Count > 2 && fields[0] == "card")
                    {
                        var holotableId = fields[1];

                        var mapping = await _context.CardImageMappings.SingleOrDefaultAsync(
                            cim => cim.Group == CardImageMapping.Holotable && cim.ExternalId == holotableId,
                            cancellationToken);
                        
                        if (mapping != null)
                        {
                            var cardImage = await _context.CardImages.SingleOrDefaultAsync(
                                ci => ci.ImageId == mapping.CardImageId,
                                cancellationToken);

                            if (cardImage != null)
                            {
                                result.Add(cardImage);
                            }
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpPost("deck-import/gemp")]
        [Consumes("application/xml")]
        [RequestSizeLimit(100 << 10)] // 100 KiB
        public async Task<IActionResult> PostDeckImportGemp(CancellationToken cancellationToken)
        {
            var result = new List<CardImage>();

            var settings = new XmlReaderSettings
            {
                Async = true
            };
            
            using (var reader = XmlReader.Create(Request.Body, settings))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.Name == "card")
                    {
                        var gempId = reader.GetAttribute("blueprintId");
                        var gempTitle = reader.GetAttribute("title");

                        var mapping = await _context.CardImageMappings.SingleOrDefaultAsync(
                            cim => cim.Group == CardImageMapping.Gemp && cim.ExternalId == gempId,
                            cancellationToken);

                        if (mapping != null)
                        {
                            var cardImage = await _context.CardImages.SingleOrDefaultAsync(
                                ci => ci.ImageId == mapping.CardImageId,
                                cancellationToken);

                            if (cardImage != null)
                            {
                                result.Add(cardImage);
                            }
                        }
                    }
                }
            }

            return Ok(result);
        }

        [HttpPost("card-inventory")]
        [Consumes("application/json")]
        [Authorize]
        public async Task<IActionResult> PostCardInventory(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            var document = await JsonDocument.ParseAsync(
                Request.Body,
                default,
                cancellationToken);
            
            try
            {
                var changes = new List<CardInventory>();
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var cardImageId = Guid.Parse(property.Name);
                    
                    var now = DateTimeOffset.Now;
                    var change = new CardInventory
                    {
                        UserId = user.Id,
                        CardImageId = cardImageId,
                        PublicLastUpdated = now,
                        PrivateLastUpdated = now
                    };

                    // TODO: finish
                }

                return Ok();
            }
            catch (Exception ex) when (ex is JsonException || ex is FormatException)
            {
                await _context.LogAsync(ex);
                return BadRequest(ex.Message);
            }
            finally
            {
                document.Dispose();
            }
        }
    }
}