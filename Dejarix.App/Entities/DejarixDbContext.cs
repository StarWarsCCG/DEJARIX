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
        public DbSet<Deck> Decks { get; set; } = null!;
        public DbSet<DeckRevision> DeckRevisions { get; set; } = null!;
        public DbSet<CardInDeckRevision> CardsInDeckRevisions { get; set; } = null!;
        public DbSet<CardInventory> CardInventories { get; set; } = null!;
        public DbSet<ExceptionLog> ExceptionLogs { get; set; } = null!;
        public DbSet<Trade> Trades { get; set; } = null!;
        public DbSet<TradeProposal> TradeProposals { get; set; } = null!;
        public DbSet<CardInTrade> CardsInTrades { get; set; } = null!;
        public DbSet<TradeMessage> TradeMessages { get; set; } = null!;

        public DejarixDbContext(DbContextOptions<DejarixDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task SeedDataAsync(string path)
        {
            JsonDocument document;

            await using (var stream = File.OpenRead(path))
                document = await JsonDocument.ParseAsync(stream);

            foreach (var cardJson in document.RootElement.EnumerateArray())
            {
                var cardImage = new CardImage
                {
                    Id = Guid.Parse(cardJson.GetProperty("ImageId").GetString()),
                    OtherId = Guid.Parse(cardJson.GetProperty("OtherImageId").GetString()),
                    IsLightSide = cardJson.GetProperty("IsLightSide").GetBoolean(),
                    IsFront = cardJson.GetProperty("IsFront").GetBoolean(),
                    Title = cardJson.GetProperty("CardName").GetString(),
                    Destiny = cardJson.GetProperty("Destiny").GetString(),
                    Expansion = cardJson.GetProperty("Expansion").GetString(),
                    InfoJson = cardJson.ToString()
                };

                cardImage.TitleNormalized = cardImage.Title.NormalizedForSearch();
                await CardImages.AddAsync(cardImage);
            }

            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ExceptionLog>().HasKey(el => new{el.ExceptionId, el.Ordinal});
            builder.Entity<CardInventory>().HasKey(co => new{co.UserId, co.CardImageId});
            builder.Entity<CardInventory>().HasOne(co => co.User);
            builder.Entity<CardInventory>().HasOne(co => co.CardImage);

            builder
                .Entity<DeckRevision>()
                .HasOne(cc => cc.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CardInDeckRevision>().HasKey(dc => new{dc.CardCollectionId, dc.CardId});
            builder.Entity<CardInDeckRevision>().HasOne(dc => dc.CardCollection);
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
            
            builder.Entity<Trade>().HasIndex(t => new {t.FirstUserId, t.Started});
            builder.Entity<Trade>().HasIndex(t => new {t.SecondUserId, t.Started});
            builder.Entity<CardInTrade>().HasKey(cit => new {cit.TradeProposalId, cit.UserId, cit.CardId});
            builder.Entity<TradeMessage>().HasIndex(tm => new {tm.TradeId, tm.TimeSent});
        }

        public async Task LogAsync(Exception exception)
        {
            var rootLog = ExceptionLog.FromException(exception, 0);
            await ExceptionLogs.AddAsync(rootLog);

            int ordinal = 1;

            for (var e = exception.InnerException; e != null; e = e.InnerException)
            {
                var log = ExceptionLog.FromException(e, ordinal++);
                await ExceptionLogs.AddAsync(log);
            }

            await SaveChangesAsync();
        }
    }
}
