using System;
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
        public DateTimeOffset RegistrationDate { get; set; } = DateTimeOffset.Now;
    }

    public class Card
    {
        [Key] public int CardId { get; set; }
        public int SwIpId { get; set; }
        public string Title { get; set; }
        public bool IsLight { get; set; }
        public string DestinyText { get; set; }
        public float LowDestinyValue { get; set; }
        public float HighDestinyValue { get; set; }
        public int CardTypeId { get; set; }
        public int ExpansionId { get; set; }
    }

    public class Resource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class DejarixDbContext : IdentityDbContext<DejarixUser, IdentityRole<Guid>, Guid>
    {
        //public DbSet<User> Users { get; set; }
        //public DbSet<Card> Cards { get; set; }

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
        }
    }
}