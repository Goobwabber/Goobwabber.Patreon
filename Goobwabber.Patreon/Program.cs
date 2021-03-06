using Goobwabber.Patreon.Configuration;
using Goobwabber.Patreon.Models;
using Goobwabber.Patreon.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Goobwabber.Patreon.Filters;

namespace Goobwabber.Patreon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder
                        .ConfigureServices((hostBuilderContext, services) =>
                            services
                                .AddOptions()
                                .AddConfiguration<AppConfiguration>("App")
                                .AddConfiguration<PatreonConfiguration>("Patreon")
                                .AddDbContext<Database>(options =>
                                    options.UseSqlite(hostBuilderContext.Configuration.GetConnectionString("SqlConnection"))
                                )
                                .AddSingleton<PatreonAPI>()
                                .AddControllers(options =>
                                    options.Filters.Add(new HttpResponseExceptionFilter())
                                )
                        )
                        .Configure(applicationBuilder =>
                            applicationBuilder
                                .UseRouting()
                                .UseEndpoints(endPointRouteBuilder => endPointRouteBuilder.MapControllers())
                        )
                )
                .UseSerilog();
    }
}
