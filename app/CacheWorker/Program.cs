using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CacheWorker
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddSystemdConsole())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();
                    services.AddHostedService<Worker>();
                });

        public static void Main(string[] args) => 
            CreateHostBuilder(args).Build().Run();
    }
}