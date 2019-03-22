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

        private static void AddRecipients(
            IDictionary<string, string> fields,
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

        public Task<HttpResponseMessage> SendEmailAsync(
            string from,
            IEnumerable<string> to,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            string subject,
            string textBody,
            string htmlBody)
        {
            var fields = new Dictionary<string, string>
            {
                ["from"] = from,
                ["subject"] = subject
            };

            AddRecipients(fields, to, "to");
            AddRecipients(fields, cc, "cc");
            AddRecipients(fields, bcc, "bcc");

            if (textBody != null)
                fields["text"] = textBody;
            
            if (htmlBody != null)
                fields["html"] = htmlBody;
            
            var content = new FormUrlEncodedContent(fields);
            return _httpClient.PostAsync(_url, content);
        }
    }
}