using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.App.Entities
{
    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types#dbcontext-and-dbset
        public DbSet<CardImage> CardImages { get; set; } = null!;
        public DbSet<CardImageMapping> CardImageMappings { get; set; } = null!;
        public DbSet<Deck> Decks { get; set; } = null!;
        public DbSet<DeckRevision> DeckRevisions { get; set; } = null!;
        public DbSet<CardInDeckRevision> CardsInDeckRevisions { get; set; } = null!;
        public DbSet<CardInventory> CardInventories { get; set; } = null!;
        public DbSet<ExceptionLog> ExceptionLogs { get; set; } = null!;
        public DbSet<Trade> Trades { get; set; } = null!;
        public DbSet<TradeProposal> TradeProposals { get; set; } = null!;
        public DbSet<CardInTrade> CardsInTrades { get; set; } = null!;
        public DbSet<TradeMessage> TradeMessages { get; set; } = null!;
        public DbSet<Game> Games { get; set; } = null!;

        public DejarixDbContext(DbContextOptions<DejarixDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        private async Task<JsonDocument> LoadDocumentAsync(string path)
        {
            await using (var stream = File.OpenRead(path))
                return await JsonDocument.ParseAsync(stream);
        }

        public async Task SeedDataAsync(string path)
        {
            using (var document = await LoadDocumentAsync(path))
            {
                foreach (var cardJson in document.RootElement.EnumerateArray())
                {
                    var cardImage = CardImage.FromJson(cardJson);
                    await CardImages.AddAsync(cardImage);

                    if (cardImage.IsFront)
                    {
                        if (cardImage.GempId != null)
                        {
                            var mapping = new CardImageMapping
                            {
                                Group = CardImageMapping.Gemp,
                                ExternalId = cardImage.GempId,
                                CardImageId = cardImage.ImageId
                            };

                            await CardImageMappings.AddAsync(mapping);
                        }

                        if (cardImage.HolotableId != null)
                        {
                            var mapping = new CardImageMapping
                            {
                                Group = CardImageMapping.Holotable,
                                ExternalId = cardImage.HolotableId,
                                CardImageId = cardImage.ImageId
                            };

                            await CardImageMappings.AddAsync(mapping);
                        }
                    }
                }

                await SaveChangesAsync();
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CardImageMapping>().HasKey(cim => new{cim.Group, cim.ExternalId});
            builder.Entity<ExceptionLog>().HasKey(el => new{el.ExceptionId, el.Ordinal});
            builder.Entity<CardInventory>().HasKey(co => new{co.UserId, co.CardImageId});
            builder.Entity<CardInventory>().HasOne(co => co.User);
            builder.Entity<CardInventory>().HasOne(co => co.CardImage);

            builder
                .Entity<DeckRevision>()
                .HasOne(cc => cc.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CardInDeckRevision>().HasKey(dc => new{dc.DeckRevisionId, dc.CardId});
            builder.Entity<CardInDeckRevision>().HasOne(dc => dc.DeckRevision);
            builder.Entity<CardInDeckRevision>().HasOne(dc => dc.Card);

            builder
                .Entity<Deck>()
                .HasOne(d => d.Revision)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder
                .Entity<Deck>()
                .HasOne(d => d.Creator)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .Entity<DeckRevision>()
                .HasOne(dr => dr.Creator)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<CardImageMapping>().HasIndex(cim => new{cim.CardImageId, cim.Group, cim.ExternalId});
            builder.Entity<Trade>().HasIndex(t => new {t.FirstUserId, t.Started});
            builder.Entity<Trade>().HasIndex(t => new {t.SecondUserId, t.Started});
            builder.Entity<CardInTrade>().HasKey(cit => new {cit.TradeProposalId, cit.UserId, cit.CardId});
            builder.Entity<TradeMessage>().HasIndex(tm => new {tm.TradeId, tm.TimeSent});
        }

        public async Task LogAsync(Exception? exception)
        {
            var id = Guid.NewGuid();
            var now = DateTimeOffset.Now;

            int ordinal = 0;

            for (var e = exception; e != null; e = e.InnerException)
            {
                var log = ExceptionLog.FromException(e, id, ordinal++, now);
                await ExceptionLogs.AddAsync(log);
            }

            await SaveChangesAsync();
        }
    }
}
