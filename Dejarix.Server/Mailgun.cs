using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dejarix.Server
{
    public class Mailgun
    {
        public const string ClientName = "Mailgun";

        public static string GetApiKey(IConfiguration configuration)
        {
            var apiKey = configuration["Mailgun:ApiKey"];
            var secret = "api:" + apiKey;
            var bytes = Encoding.UTF8.GetBytes(secret);
            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }

        private readonly IHttpClientFactory _factory;
        private readonly string _url;

        public Mailgun(
            IConfiguration configuration,
            IHttpClientFactory factory)
        {
            _factory = factory;
            _url = configuration["Mailgun:Api"];
        }

        private static void AddRecipients(
            IDictionary<string, string> fields,
            IEnumerable<string> recipients,
            string key)
        {
            if (recipients != null)
            {
                var formattedRecipients = string.Join(", ", recipients);

                if (formattedRecipients.Length > 0)
                    fields[key] = formattedRecipients;
            }
        }

        public async Task SendEmailAsync(
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
            var client = _factory.CreateClient(ClientName);
            var response = await client.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}