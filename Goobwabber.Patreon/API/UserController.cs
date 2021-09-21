using Goobwabber.Patreon.Configuration;
using Goobwabber.Patreon.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net.Http;
using System.Threading.Tasks;

namespace Goobwabber.Patreon.API
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private const string PatreonAPI = "https://www.patreon.com/api/";

        private readonly ILogger _logger = Log.ForContext<AuthenticateController>();
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly PatreonConfiguration _patreonConfiguration;
        private readonly DataContext _dataContext;

        public UserController(
            PatreonConfiguration patreonConfiguration,
            DataContext dataContext)
        {
            _patreonConfiguration = patreonConfiguration;
            _dataContext = dataContext;
        }
    }

    [HttpGet]
    public async Task Get(string userid)
    {
        
    }
}
