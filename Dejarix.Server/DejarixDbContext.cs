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

    public class CardInventory
    {
        public Guid UserId { get; set; }
        public DejarixUser User { get; set; }
        public Guid CardImageId { get; set; }
        public CardImage CardImage { get; set; }
        public int PublicHaveCount { get; set; }
        public int PublicWantCount { get; set; }
        public int PrivateHaveCount { get; set; }
        public int PrivateWantCount { get; set; }
    }

    public class CardImage
    {
        [Key] public Guid Id { get; set; }
        public Guid OtherId { get; set; }
        public bool IsLightSide { get; set; }
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
        public Guid RevisionId { get; set; }
        public DeckRevision Revision { get; set; }
    }

    public class DeckRevision
    {
        [Key] public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public DeckRevision Parent { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public Guid CreatorId { get; set; }
        public DejarixUser Creator { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class CardInDeckRevision
    {
        public Guid CardCollectionId { get; set; }
        public DeckRevision CardCollection { get; set; }
        public Guid CardId { get; set; }
        public CardImage Card { get; set; }
        public int StartCount { get; set; }
        public int CardCount { get; set; }
        public int OutsideCount { get; set; }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<CardImage> CardImages { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<DeckRevision> DeckRevisions { get; set; }
        public DbSet<CardInDeckRevision> CardsInDeckRevisions { get; set; }
        public DbSet<CardInventory> CardInventories { get; set; }

        public DejarixDbContext(
            DbContextOptions<DejarixDbContext> options) : base(options)
        {
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
    }
}