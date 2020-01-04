using System;

namespace Dejarix.App.Models
{
    public class ResetViewModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string Success { get; set; } = string.Empty;
    }
}