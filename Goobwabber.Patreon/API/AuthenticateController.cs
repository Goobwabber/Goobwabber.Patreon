using Goobwabber.Patreon.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
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
        public async Task Get(string code, string state)
        {
            if (code is null || state is null)
                throw new HttpResponseException(400);

            var validationResponse = await _patreonAPI.ValidateOAuth(code);

            if (validationResponse.error is not null)
            {
                throw new HttpResponseException(500);
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
        }
    }
}
