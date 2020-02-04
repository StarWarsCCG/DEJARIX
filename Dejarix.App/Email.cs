using System.Collections.Generic;
using System.Collections.Immutable;

namespace Dejarix.App
{
    public class Email
    {
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TextBody { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public ImmutableArray<string> To { get; set; } = ImmutableArray<string>.Empty;
        public ImmutableArray<string> Cc { get; set; } = ImmutableArray<string>.Empty;
        public ImmutableArray<string> Bcc { get; set; } = ImmutableArray<string>.Empty;

        public IDictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>
            {
                ["from"] = From,
                ["subject"] = Subject
            };

            AddRecipients(result, To, "to");
            AddRecipients(result, Cc, "cc");
            AddRecipients(result, Bcc, "bcc");

            result["text"] = TextBody;
            result["html"] = HtmlBody;
            
            return result;
        }

        private static void AddRecipients(
            Dictionary<string, string> fields,
            ImmutableArray<string> recipients,
            string key)
        {
            var formattedRecipients = string.Join(", ", recipients);

            if (formattedRecipients.Length > 0)
                fields[key] = formattedRecipients;
        }
    }
}