using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public ImmutableArray<string> DefaultBcc { get; }

        public Mailgun(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _url = configuration["Mailgun:Api"];
            _httpClient = httpClient;

            DefaultSender = configuration["SenderEmailAddress"];
            DefaultBcc = configuration
                .GetSection("DefaultBcc")
                .GetChildren()
                .Select(c => c.Value)
                .ToImmutableArray();

            var apiKey = configuration["Mailgun:ApiKey"];
            var secret = "api:" + apiKey;
            var bytes = Encoding.UTF8.GetBytes(secret);
            var base64 = Convert.ToBase64String(bytes);
            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    base64);
        }

        public async Task<string> SendEmailAsync(Email email)
        {
            var fields = email.ToDictionary();
            
            using (var content = new FormUrlEncodedContent(fields))
            using (var response = await _httpClient.PostAsync(_url, content))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}