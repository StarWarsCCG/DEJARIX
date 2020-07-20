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
        public async Task<IActionResult> DeckRevision(
            Guid deckRevisionId,
            CancellationToken cancellationToken)
        {
            var deckRevision = await _context.DeckRevisions
                .Include(dr => dr.Cards)
                .SingleOrDefaultAsync(dr => dr.DeckRevisionId == deckRevisionId, cancellationToken);

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
        public async Task<IActionResult> Deck(
            Guid deckId,
            CancellationToken cancellationToken)
        {
            var deck = await _context.Decks.SingleOrDefaultAsync(
                d => d.DeckId == deckId, cancellationToken);

            if (deck is null)
            {
                return NotFound();
            }
            else
            {
                return await DeckRevision(deck.RevisionId, cancellationToken);
            }
        }

        [HttpPost("deck")]
        [Authorize]
        [RequestSizeLimit(32 << 10)] // 32 KiB
        public async Task<IActionResult> PostDeck(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            using var document = await JsonDocument.ParseAsync(
                Request.Body,
                default,
                cancellationToken);
            
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return BadRequest("Expected object");
            
            if (!root.TryGetProperty("insideCards", out var insideCardsJson))
                return BadRequest("Missing 'insideCards' property.");
            
            if (insideCardsJson.ValueKind != JsonValueKind.Object)
                return BadRequest("'insideCards' property must be object.");
            
            var insideCards = new Dictionary<Guid, int>();
            foreach (var property in insideCardsJson.EnumerateObject())
            {
                if (!Guid.TryParse(property.Name, out var cardImageId))
                    return BadRequest($"Card ID '{property.Name}' is not a valid UUID.");
                
                if (property.Value.ValueKind != JsonValueKind.Number)
                    return BadRequest($"Missing quantity for card {property.Name}.");
                
                insideCards.Add(cardImageId, property.Value.GetInt32());
            }

            if (!root.TryGetProperty("outsideCards", out var outsideCardsJson))
                return BadRequest("Missing 'outsideCards' property.");
            
            if (outsideCardsJson.ValueKind != JsonValueKind.Object)
                return BadRequest("'outsideCards' property must be object.");
            
            var outsideCards = new Dictionary<Guid, int>();
            foreach (var property in outsideCardsJson.EnumerateObject())
            {
                if (!Guid.TryParse(property.Name, out var cardImageId))
                    return BadRequest($"Card ID '{property.Name}' is not a valid UUID.");
                
                if (property.Value.ValueKind != JsonValueKind.Number)
                    return BadRequest($"Missing quantity for card {property.Name}.");
                
                outsideCards.Add(cardImageId, property.Value.GetInt32());
            }
            
            var now = DateTimeOffset.Now;

            var deckRevision = new DeckRevision
            {
                DeckRevisionId = Guid.NewGuid(),
                CreatorId = user.Id,
                CreationDate = now
            };

            _context.DeckRevisions.Add(deckRevision);
            await _context.SaveChangesAsync(cancellationToken);

            var deck = new Deck
            {
                DeckId = Guid.NewGuid(),
                IsPublic = true,
                CreatorId = user.Id,
                CreationDate = now,
                ModifiedDate = now,
                RevisionId = deckRevision.DeckRevisionId
            };

            _context.Decks.Add(deck);

            _context.CardsInDeckRevisions.AddRange(insideCards.Select(
                pair => new CardInDeckRevision
                {
                    DeckRevisionId = deckRevision.DeckRevisionId,
                    CardId = pair.Key,
                    InsideCount = pair.Value
                }));
            
            _context.CardsInDeckRevisions.AddRange(outsideCards.Select(
                pair => new CardInDeckRevision
                {
                    DeckRevisionId = deckRevision.DeckRevisionId,
                    CardId = pair.Key,
                    OutsideCount = pair.Value
                }));

            await _context.SaveChangesAsync(cancellationToken);
            
            return Ok(deck.DeckId.ToString());
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
        [RequestSizeLimit(32 << 10)] // 32 KiB
        public async Task<IActionResult> PostDeckImportHolotable(CancellationToken cancellationToken)
        {
            var countByHolotableId = new Dictionary<string, int>();

            using (var reader = new StreamReader(Request.Body))
            {
                var fields = new List<string>();
                
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var line = await reader.ReadLineAsync();

                    if (line is null)
                        break;
                    
                    fields.Clear();
                    Holotable.AddFields(fields, line);

                    if (fields.Count > 2 && fields[0] == "card")
                    {
                        var holotableId = fields[1];

                        if (!int.TryParse(fields[2], out var count))
                            return BadRequest($"Card \"{holotableId}\" has invalid count.");
                        
                        if (countByHolotableId.ContainsKey(holotableId))
                            return BadRequest($"Card \"{holotableId}\" specified twice.");
                        
                        countByHolotableId.Add(holotableId, count);
                    }
                }
            }

            var keys = countByHolotableId.Keys.ToArray();
            var mappings = await _context.CardImages
                .Where(ci => keys.Contains(ci.HolotableId))
                .Select(ci => new {ci.ImageId, ci.HolotableId})
                .ToListAsync(cancellationToken);
            
            var imageIdByHolotableId = mappings.ToDictionary(
                result => result.HolotableId,
                result => result.ImageId);

            var missing = countByHolotableId.FirstOrDefault(pair => !imageIdByHolotableId.ContainsKey(pair.Key)).Key;

            if (missing != null)
                return BadRequest($"Unrecognized card ID: {missing}");
            
            var result = countByHolotableId.ToDictionary(
                pair => imageIdByHolotableId[pair.Key].ToString(),
                pair => pair.Value);

            return Ok(result);
        }

        [HttpPost("deck-import/gemp")]
        [Consumes("application/xml")]
        [RequestSizeLimit(32 << 10)] // 32 KiB
        public async Task<IActionResult> PostDeckImportGemp(CancellationToken cancellationToken)
        {
            var countByGempId = new Dictionary<string, int>();

            var settings = new XmlReaderSettings
            {
                Async = true
            };
            
            using (var reader = XmlReader.Create(Request.Body, settings))
            {
                while (await reader.ReadAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (reader.Name == "card")
                    {
                        var gempId = reader.GetAttribute("blueprintId");
                        var gempTitle = reader.GetAttribute("title");

                        countByGempId.TryGetValue(gempId, out var count);
                        countByGempId[gempId] = count + 1;
                    }
                }
            }

            var keys = countByGempId.Keys.ToArray();
            var mappings = await _context.CardImages
                .Where(ci => keys.Contains(ci.GempId))
                .Select(ci => new {ci.ImageId, ci.GempId})
                .ToListAsync(cancellationToken);
            
            var imageIdByGempId = mappings.ToDictionary(
                result => result.GempId,
                result => result.ImageId);
            
            var missing = countByGempId.FirstOrDefault(pair => !imageIdByGempId.ContainsKey(pair.Key)).Key;

            if (missing != null)
                return BadRequest($"Unrecognized card ID: {missing}");
            
            var result = countByGempId.ToDictionary(
                pair => imageIdByGempId[pair.Key].ToString(),
                pair => pair.Value);

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