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
        public CardImage Card { get; set; }
        public int HaveCount { get; set; }
        public int WantCount { get; set; }
    }

    public class CardImage
    {
        [Key] public Guid Id { get; set; }
        public Guid OtherId { get; set; }
        public bool IsFront { get; set; }
        public string Title { get; set; }
        public string TitleNormalized { get; set; }
        public string Destiny { get; set; }
        public string Expansion { get; set; }
        public string InfoJson { get; set; }
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
        public CardImage Card { get; set; }
        public int StartCount { get; set; }
        public int CardCount { get; set; }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<CardImage> CardImages { get; set; }
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
            var text = File.ReadAllText(path);
            var json = JArray.Parse(text);

            foreach (JObject cardJson in json)
            {
                var cardImage = new CardImage
                {
                    Id = Guid.Parse((string)cardJson["ImageId"]),
                    OtherId = Guid.Parse((string)cardJson["OtherImageId"]),
                    Title = (string)cardJson["CardName"],
                    Destiny = (string)cardJson["Destiny"],
                    Expansion = (string)cardJson["Expansion"],
                    InfoJson = cardJson.ToString(Formatting.Indented)
                };

                cardImage.TitleNormalized = cardImage.Title.SearchNormalized();
                CardImages.Add(cardImage);
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