using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace CacheWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static Lazy<ConnectionMultiplexer> _lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return _lazyConnection.Value;
            }
        }
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                //Key vault service URL
                var kvUri = "https://kv-redis-keyvault-proxy.vault.azure.net/";
                var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

                // <getsecret>
                KeyVaultSecret primary = client.GetSecret("redis-connection-string-primary");
                KeyVaultSecret secondary = client.GetSecret("redis-connection-string-secondary");

                // </getsecret>
                //displaying the secret URL
                Console.WriteLine("Your secret is '" + primary.Value + "'.");
                Console.WriteLine("Your secret is '" + secondary.Value + "'.");

                return ConnectionMultiplexer.Connect($"{primary.Value},{secondary.Value}"); // this might not be kosher
            });
        }

        public override void Dispose()
        {
            _lazyConnection.Value.Dispose();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IDatabase cache = Connection.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                
                // Perform cache operations using the cache object...

                // Simple PING command
                string cacheCommand = "PING";
                Console.WriteLine("\nCache command  : " + cacheCommand);
                Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());

                // Simple get and put of integral data types into the cache
                cacheCommand = "GET Message";
                Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
                Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

                cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
                Console.WriteLine("\nCache command  : " + cacheCommand + " or StringSet()");
                Console.WriteLine("Cache response : " + cache.StringSet("Message", "Hello! The cache is working from a .NET Core console app!").ToString());

                // Demonstrate "SET Message" executed as expected...
                cacheCommand = "GET Message";
                Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
                Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

                // Get the client list, useful to see if connection list is growing...
                cacheCommand = "CLIENT LIST";
                Console.WriteLine("\nCache command  : " + cacheCommand);
                Console.WriteLine("Cache response : \n" + cache.Execute("CLIENT", "LIST").ToString().Replace("id=", "id="));

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
