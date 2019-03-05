using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Dejarix.Server
{
    public class DejarixUser : IdentityUser<Guid>
    {
        public DateTimeOffset RegistrationDate { get; set; }
    }

    public class CardOwnership
    {
        public Guid UserId { get; set; }
        public DejarixUser User { get; set; }
        public Guid CardId { get; set; }
        public Card Card { get; set; }
        public int HaveCount { get; set; }
        public int WantCount { get; set; }
    }

    public class Card
    {
        [Key] public Guid FrontImageId { get; set; }
        public Guid BackImageId { get; set; }
        public string SwIpJson { get; set; }
    }

    public class CardFace
    {
        [Key] public Guid ImageId { get; set; }
        public string Title { get; set; }
        public string TitleNormalized { get; set; }
        public string Destiny { get; set; }
        public string Expansion { get; set; }
    }

    public class Deck
    {
        [Key] public Guid Id { get; set; }
        public bool IsPublic { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? DeletionDate { get; set; }
        public Guid CreatorId { get; set; }
        public DejarixUser Creator { get; set; }
        public Guid CardCollectionId { get; set; }
        public CardCollection CardCollection { get; set; }
    }

    public class CardCollection
    {
        [Key] public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public CardCollection Parent { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class CardInCollection
    {
        public Guid CardCollectionId { get; set; }
        public CardCollection CardCollection { get; set; }
        public Guid CardId { get; set; }
        public Card Card { get; set; }
        public int StartCount { get; set; }
        public int CardCount { get; set; }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardFace> CardFaces { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<CardCollection> CardCollections { get; set; }
        public DbSet<CardInCollection> CardsInCollections { get; set; }
        public DbSet<CardOwnership> OwnedCards { get; set; }

        public DejarixDbContext(
            DbContextOptions<DejarixDbContext> options) : base(options)
        {
        }

        public void SeedData(string path)
        {
            var darkImageId = Guid.Parse("60cca2dd-a989-4c55-8a7b-cd6b86a95ce5");
            var lightImageId = Guid.Parse("14c10fe6-199e-4b7d-9cea-1c7247e42d3e");

            var text = File.ReadAllText(path);
            var json = JArray.Parse(text);

            foreach (JObject cardJson in json)
            {
                bool isLightSide = "Light".Equals((string)cardJson["Grouping"]);
                bool isObjective = Guid.TryParse((string)cardJson["BackImageId"], out var backImageId);
                if (!isObjective)
                    backImageId = isLightSide ? lightImageId : darkImageId;

                var card = new Card
                {
                    FrontImageId = Guid.Parse((string)cardJson["FrontImageId"]),
                    BackImageId = backImageId,
                    SwIpJson = cardJson.ToString(Formatting.Indented)
                };

                Cards.Add(card);

                if (isObjective)
                {
                    var frontCardFace = new CardFace
                    {
                        ImageId = Guid.Parse((string)cardJson["FrontImageId"]),
                        Title = (string)cardJson["ObjectiveFrontName"],
                        Destiny = "0"
                    };

                    frontCardFace.TitleNormalized = frontCardFace.Title.SearchNormalized();

                    var backCardFace = new CardFace
                    {
                        ImageId = Guid.Parse((string)cardJson["BackImageId"]),
                        Title = (string)cardJson["ObjectiveBackName"],
                        Destiny = "7"
                    };

                    backCardFace.TitleNormalized = backCardFace.Title.SearchNormalized();

                    CardFaces.Add(frontCardFace);
                    CardFaces.Add(backCardFace);
                }
                else
                {
                    var cardFace = new CardFace
                    {
                        ImageId = Guid.Parse((string)cardJson["FrontImageId"]),
                        Title = (string)cardJson["CardName"],
                        Destiny = (string)cardJson["Destiny"]
                    };

                    cardFace.TitleNormalized = cardFace.Title.SearchNormalized();

                    CardFaces.Add(cardFace);
                }
            }

            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CardOwnership>().HasKey(co => new{co.UserId, co.CardId});
            builder.Entity<CardOwnership>().HasOne(co => co.User);
            builder.Entity<CardOwnership>().HasOne(co => co.Card);

            builder
                .Entity<CardCollection>()
                .HasOne(cc => cc.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CardInCollection>().HasKey(dc => new{dc.CardCollectionId, dc.CardId});
            builder.Entity<CardInCollection>().HasOne(dc => dc.CardCollection);
            builder.Entity<CardInCollection>().HasOne(dc => dc.Card);

            builder
                .Entity<Deck>()
                .HasOne(d => d.CardCollection)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder
                .Entity<Deck>()
                .HasOne(d => d.Creator);
        }
    }
}