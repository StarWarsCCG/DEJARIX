using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.App.Entities
{
    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        public static void Create(IServiceProvider serviceProvider, DbContextOptionsBuilder builder)
        {
            var factory = serviceProvider.GetService<ConnectionFactory>();
            factory.BuildOptions(builder);
        }

        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types#dbcontext-and-dbset
        public DbSet<CardImage> CardImages { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<DeckRevision> DeckRevisions { get; set; }
        public DbSet<CardInDeckRevision> CardsInDeckRevisions { get; set; }
        public DbSet<CardInventory> CardInventories { get; set; }
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeProposal> TradeProposals { get; set; }
        public DbSet<CardInTrade> CardsInTrades { get; set; }
        public DbSet<TradeMessage> TradeMessages { get; set; }
        public DbSet<Game> Games { get; set; }

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
                }

                await SaveChangesAsync();
            }
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
            
            builder.Entity<Trade>().HasIndex(t => new {t.FirstUserId, t.Started});
            builder.Entity<Trade>().HasIndex(t => new {t.SecondUserId, t.Started});
            builder.Entity<CardInTrade>().HasKey(cit => new {cit.TradeProposalId, cit.UserId, cit.CardId});
            builder.Entity<TradeMessage>().HasIndex(tm => new {tm.TradeId, tm.TimeSent});
            builder.Entity<CardImage>().HasIndex(ci => new{ci.GempId});
            builder.Entity<CardImage>().HasIndex(ci => new{ci.HolotableId});
        }

        public async Task LogAsync(Exception exception)
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
