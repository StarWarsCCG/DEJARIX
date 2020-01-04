using System;

namespace Dejarix.App.Models
{
    public class RegisterViewModel
    {
        public string PreviousUsername { get; set; } = string.Empty;
        public string PreviousEmail { get; set; } = string.Empty;
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string Success { get; set; } = string.Empty;
    }
}
