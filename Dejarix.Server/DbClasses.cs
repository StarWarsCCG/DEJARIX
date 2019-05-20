using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public class ExceptionLog
    {
        public Guid Id { get; set; }
        public int Ordinal { get; set; }
        public DateTimeOffset ExceptionDate { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }

        public static ExceptionLog FromException(Exception exception)
        {
            return new ExceptionLog
            {
                ExceptionType = exception.GetType().ToString(),
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace
            };
        }
    }
}