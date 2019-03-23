using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dejarix.Server
{
    public class Mailgun
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public Mailgun(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _url = configuration["Mailgun:Api"];
            _httpClient = httpClient;

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
        public string From { get; set; }
        public string Subject { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public IEnumerable<string> To { get; set; }
        public IEnumerable<string> Cc { get; set; }
        public IEnumerable<string> Bcc { get; set; }

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

            if (TextBody != null)
                result["text"] = TextBody;
            
            if (HtmlBody != null)
                result["html"] = HtmlBody;
            
            return result;
        }

        private static void AddRecipients(
            Dictionary<string, string> fields,
            IEnumerable<string> recipients,
            string key)
        {
            if (recipients != null)
            {
                var formattedRecipients = string.Join(
                    ", ",
                    recipients.Where(r => !string.IsNullOrWhiteSpace(r)));

                if (formattedRecipients.Length > 0)
                    fields[key] = formattedRecipients;
            }
        }
    }
}