using Goobwabber.Patreon.Configuration;
using Goobwabber.Patreon.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Goobwabber.Patreon.API
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticateController : ControllerBase
    {
        private const string PatreonAPI = "https://www.patreon.com/api/";

        private readonly ILogger _logger = Log.ForContext<AuthenticateController>();
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly PatreonConfiguration _patreonConfiguration;
        private readonly DataContext _dataContext;

        public AuthenticateController(
            PatreonConfiguration patreonConfiguration,
            DataContext dataContext)
        {
            _patreonConfiguration = patreonConfiguration;
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task Get(string code, string state)
        {
            var validationRequest = await _httpClient.PostAsync(PatreonAPI + "oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" },
                { "client_id", _patreonConfiguration.ClientId },
                { "client_secret", _patreonConfiguration.ClientSecret },
                { "redirect_uri", _patreonConfiguration.RedirectUri }
            }));

            var validationResponse = await validationRequest.Content.ReadAsAsync<ValidationResponse>();

            if (validationResponse.error is not null)
                _logger.Error($"{validationResponse.error}: {validationResponse.error_description}");

            DataContext.User user = await _dataContext.Users.FindAsync(state);
            if (user is null)
            {
                user = new DataContext.User
                {
                    UserId = state
                };

                _dataContext.Users.Add(user);
            }

            user.AccessToken = validationResponse.access_token;
            user.RefreshToken = validationResponse.refresh_token;
            user.TokenExpiry = DateTime.Now.AddSeconds(validationResponse.expires_in);

            _ = _dataContext.SaveChangesAsync();
        }

        public class ValidationResponse
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
