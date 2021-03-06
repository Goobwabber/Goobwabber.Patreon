using Goobwabber.Patreon.Configuration;
using Goobwabber.Patreon.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Goobwabber.Patreon.API
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger = Log.ForContext<UserController>();
        private readonly AppConfiguration _configuration;
        private readonly PatreonAPI _patreonAPI;
        private readonly Database _database;

        public UserController(
            AppConfiguration configuration,
            PatreonAPI patreonAPI,
            Database database)
        {
            _configuration = configuration;
            _patreonAPI = patreonAPI;
            _database = database;
        }

        [HttpGet]
        public async Task<UserResponse> Get(string userid)
        {
            Database.User user = _database.Users.Find(userid);
            if (user is null)
                throw new HttpResponseException(400);

            if (user.TokenExpiry < DateTime.Now)
                throw new HttpResponseException(500);

            if (user.LastCheckDate.AddSeconds(_configuration.IdentityRefreshTime) > DateTime.Now )
                return new UserResponse
                {
                    UserId = user.UserId,
                    Patron = user.Patron
                };

            var identityResponse = await _patreonAPI.GetIdentity(user.AccessToken);

            user.Patron = identityResponse.data.relationships.memberships.data.Any() &&
                identityResponse.data.relationships.memberships.data.First().type == "member";

            user.LastCheckDate = DateTime.Now;

            await _database.SaveChangesAsync();

            return new UserResponse
            {
                UserId = user.UserId,
                Patron = user.Patron
            };
        }

        public class UserResponse
        {
            public string UserId { get; set; }
            public bool Patron { get; set; }
        }
    }
}
