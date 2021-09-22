using Goobwabber.Patreon.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
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
        private readonly PatreonAPI _patreonAPI;
        private readonly Database _database;

        public AuthenticateController(
            PatreonAPI patreonAPI,
            Database database)
        {
            _patreonAPI = patreonAPI;
            _database = database;
        }

        [HttpGet]
        public async Task<AuthenticationResponse> Get(string code, string state)
        {
            var validationResponse = await _patreonAPI.ValidateOAuth(code);

            if (validationResponse.error is not null)
            {
                _logger.Error($"Patreon Validation Error: {validationResponse.error} {validationResponse.error_description}");
                return new AuthenticationResponse
                {
                    Success = false,
                    Error = validationResponse.error,
                    ErrorDescription = validationResponse.error_description
                };
            }

            Database.User user = await _database.Users.FindAsync(state);
            if (user is null)
            {
                user = new Database.User
                {
                    UserId = state
                };

                _database.Users.Add(user);
            }

            user.AccessToken = validationResponse.access_token;
            user.RefreshToken = validationResponse.refresh_token;
            user.TokenExpiry = DateTime.Now.AddSeconds(validationResponse.expires_in);

            await _database.SaveChangesAsync();

            return new AuthenticationResponse
            {
                Success = true
            };
        }

        public class AuthenticationResponse {
            public bool Success { get; set; }

            public string Error { get; set; }
            public string ErrorDescription { get; set; }
        }
    }
}
