using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dejarix.App
{
    public class Mailgun
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public string DefaultSender { get; }
        public string[] DefaultBcc { get; }

        public Mailgun(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _url = configuration["Mailgun:Api"];
            _httpClient = httpClient;

            DefaultSender = configuration["SenderEmailAddress"];
            DefaultBcc = configuration.GetSection("DefaultBcc").GetChildren().Select(c => c.Value).ToArray();

            var apiKey = configuration["Mailgun:ApiKey"];
            var secret = "api:" + apiKey;
            var bytes = Encoding.UTF8.GetBytes(secret);
            var base64 = Convert.ToBase64String(bytes);
            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    base64);
        }

        public Task<HttpResponseMessage> SendEmailAsync(Email email)
        {
            var fields = email.ToDictionary();
            var content = new FormUrlEncodedContent(fields);
            return _httpClient.PostAsync(_url, content);
        }
    }

    public class Email
    {
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TextBody { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string[] To { get; set; } = Array.Empty<string>();
        public string[] Cc { get; set; } = Array.Empty<string>();
        public string[] Bcc { get; set; } = Array.Empty<string>();

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
            string[] recipients,
            string key)
        {
            var formattedRecipients = string.Join(", ", recipients);

            if (formattedRecipients.Length > 0)
                fields[key] = formattedRecipients;
        }
    }
}