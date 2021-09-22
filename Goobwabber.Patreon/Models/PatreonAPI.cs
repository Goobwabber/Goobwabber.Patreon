using Goobwabber.Patreon.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Goobwabber.Patreon.Models
{
    public class PatreonAPI
    {
        private const string PatreonEndpoint = "https://www.patreon.com/api/";

        private readonly ILogger _logger = Log.ForContext<PatreonAPI>();
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly PatreonConfiguration _patreonConfiguration;

        public PatreonAPI(
            PatreonConfiguration patreonConfiguration)
        {
            _patreonConfiguration = patreonConfiguration;
        }

        public async Task<OAuthValidationResponse> ValidateOAuth(string code)
        {
            _logger.Information($"Validating Patreon OAuth with {code}.");

            var validationRequest = await _httpClient.PostAsync(PatreonEndpoint + "oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" },
                { "client_id", _patreonConfiguration.ClientId },
                { "client_secret", _patreonConfiguration.ClientSecret },
                { "redirect_uri", _patreonConfiguration.RedirectUri }
            }));

            _logger.Information($"Received {nameof(OAuthValidationResponse)} ({await validationRequest.Content.ReadAsStringAsync()})");
            return await validationRequest.Content.ReadAsAsync<OAuthValidationResponse>();
        }

        public async Task<IdentityResponse> GetIdentity(string accessToken)
        {
            var identityRequestMessage = new HttpRequestMessage(HttpMethod.Get, PatreonEndpoint + "/oauth2/v2/identity?include=memberships");
            identityRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var identityRequest = await _httpClient.SendAsync(identityRequestMessage);

            _logger.Information($"Received {nameof(IdentityResponse)} ({await identityRequest.Content.ReadAsStringAsync()})");
            identityRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await identityRequest.Content.ReadAsAsync<IdentityResponse>();
        }

        public class IdentityResponse
        {
            public Data data { get; set; }
            public Links links { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }

            public class Data
            {
                public Attributes attributes { get; set; }
                public string id { get; set; }
                public Relationships relationships { get; set; }
                public string type { get; set; }

                public class Attributes
                {
                    public string email { get; set; }
                    public string full_name { get; set; }
                }

                public class Relationships
                {
                    public Memberships memberships { get; set; }

                    public class Memberships
                    {
                        public Data[] data { get; set; }

                        public class Data
                        {
                            public string id { get; set; }
                            public string type { get; set; }
                        }
                    }
                }
            }

            public class Links
            {
                public string self { get; set; }
            }
        }

        public class OAuthValidationResponse
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public long expires_in { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }

            public string error { get; set; }
            public string error_description { get; set; }
        }
    }
}
