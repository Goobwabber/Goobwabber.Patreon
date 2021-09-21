using Goobwabber.Patreon.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Goobwabber.Patreon.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSerilog(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices((hostBuilderContext, services) =>
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();

                    var section = hostBuilderContext.Configuration.GetSection("Serilog");
                    var configuration = section is not null
                        ? section.Get<SerilogConfiguration>()
                        : new SerilogConfiguration();

                    var loggerConfiguration = new LoggerConfiguration();

                    loggerConfiguration.MinimumLevel.Is(configuration.MinimumLevel.Default);
                    foreach (var kvp in configuration.MinimumLevel.Overrides)
                        loggerConfiguration.MinimumLevel.Override(kvp.Key, kvp.Value);

                    if (configuration.WriteToConsole)
                        loggerConfiguration.WriteTo.Console();

                    if (configuration.File is not null && configuration.File.Path is not null)
                        loggerConfiguration.WriteTo.File(
                            configuration.File.Path,
                            fileSizeLimitBytes: configuration.File.FileSizeLimitBytes,
                            retainedFileCountLimit: configuration.File.RetainedFileCountLimit,
                            rollingInterval: RollingInterval.Day,
                            buffered: configuration.File.Buffered
                        );

                    Log.Logger = loggerConfiguration.CreateLogger();
                    loggingBuilder.AddSerilog(Log.Logger, true);
                })
            );
    }
}
