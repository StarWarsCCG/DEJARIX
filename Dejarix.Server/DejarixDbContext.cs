using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        public int CardId { get; set; }
        public Card Card { get; set; }
        public int Quantity { get; set; }
    }

    public class Card
    {
        [Key] public int Id { get; set; }
        public int SwIpId { get; set; }
        public string Title { get; set; }
        public bool IsLightSide { get; set; }
        public string DestinyText { get; set; }
        public float LowDestinyValue { get; set; }
        public float HighDestinyValue { get; set; }
        public int CardTypeId { get; set; }
        public int ExpansionId { get; set; }
    }

    public class Deck
    {
        [Key] public Guid Id { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? DeletionDate { get; set; }
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
        public int CardId { get; set; }
        public Card Card { get; set; }
        public int StartCount { get; set; }
        public int CardCount { get; set; }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<CardCollection> CardCollections { get; set; }
        public DbSet<CardInCollection> CardsInCollections { get; set; }
        public DbSet<CardOwnership> OwnedCards { get; set; }

        public DejarixDbContext(
            DbContextOptions<DejarixDbContext> options) : base(options)
        {
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
        }
    }
}