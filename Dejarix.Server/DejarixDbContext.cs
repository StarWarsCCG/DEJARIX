using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dejarix.Server
{
    public static class DejarixExtensions
    {
        public static DejarixDbContext GetDbContext(this ControllerBase controller)
        {
            return controller
                .HttpContext
                .RequestServices
                .GetService<DejarixDbContext>();
        }

        public static IApplicationBuilder UseDejarixExceptionLogger(
            this IApplicationBuilder builder)
        {
            return builder.Use(next =>
            {
                return async context =>
                {
                    try
                    {
                        await next(context);
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            using (var dbContext = context.RequestServices.GetService<DejarixDbContext>())
                                await dbContext.LogAsync(exception);
                        }
                        catch (Exception dbException)
                        {
                            for (var e = dbException; e != null; e = e.InnerException)
                            {
                                Console.WriteLine(e.GetType());
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.StackTrace);
                            }
                        }

                        throw;
                    }
                };
            });
        }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        private readonly DejarixLoggerProvider _loggerProvider;

        public DbSet<CardImage> CardImages { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<DeckRevision> DeckRevisions { get; set; }
        public DbSet<CardInDeckRevision> CardsInDeckRevisions { get; set; }
        public DbSet<CardInventory> CardInventories { get; set; }
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        public DejarixDbContext(
            DejarixLoggerProvider loggerProvider,
            DbContextOptions<DejarixDbContext> options) : base(options)
        {
            _loggerProvider = loggerProvider;
        }

        public void SeedData(string path)
        {
            JArray json;
            
            using (var textReader = new StreamReader(path))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                json = JArray.Load(jsonReader);
            }

            foreach (JObject cardJson in json)
            {
                var cardImage = new CardImage
                {
                    Id = Guid.Parse((string)cardJson["ImageId"]),
                    OtherId = Guid.Parse((string)cardJson["OtherImageId"]),
                    IsLightSide = (bool)cardJson["IsLightSide"],
                    IsFront = (bool)cardJson["IsFront"],
                    Title = (string)cardJson["CardName"],
                    Destiny = (string)cardJson["Destiny"],
                    Expansion = (string)cardJson["Expansion"],
                    InfoJson = cardJson.ToString(Formatting.Indented)
                };

                cardImage.TitleNormalized = cardImage.Title.NormalizedForSearch();
                CardImages.Add(cardImage);
            }

            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);

            var lf = new LoggerFactory();
            lf.AddProvider(_loggerProvider);
            builder.UseLoggerFactory(lf);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ExceptionLog>().HasKey(el => new{el.Id, el.Ordinal});
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
        }

        public async Task LogAsync(Exception exception)
        {
            if (exception != null)
            {
                var rootLog = ExceptionLog.FromException(exception);
                rootLog.Id = Guid.NewGuid();
                rootLog.ExceptionDate = DateTimeOffset.Now;
                await ExceptionLogs.AddAsync(rootLog);

                int ordinal = 1;

                for (var e = exception.InnerException; e != null; e = e.InnerException)
                {
                    var log = ExceptionLog.FromException(e);
                    log.Id = rootLog.Id;
                    log.Ordinal = ordinal++;
                    log.ExceptionDate = rootLog.ExceptionDate;
                    await ExceptionLogs.AddAsync(log);
                }

                await SaveChangesAsync();
            }
        }
    }
}