using System;
using Microsoft.AspNetCore.Identity;

namespace Dejarix.App.Entities
{
    public class DejarixUser : IdentityUser<Guid>
    {
        public DateTimeOffset RegistrationDate { get; set; }
    }
}