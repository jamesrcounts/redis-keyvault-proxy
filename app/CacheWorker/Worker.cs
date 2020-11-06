using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
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
        private IMemoryCache _cache;
        private readonly ILogger<Worker> _logger;
        private static Lazy<ConnectionMultiplexer> _lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return _lazyConnection.Value;
            }
        }

        
        public Worker(ILogger<Worker> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
        }

        private ConnectionMultiplexer SetupConnection()
        {
            //Key vault service URL
            var kvUri = "https://kv-redis-keyvault-proxy.vault.azure.net/";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            KeyVaultSecret primary = GetSecret(client, "redis-connection-string-primary");
            KeyVaultSecret secondary = GetSecret(client, "redis-connection-string-secondary");

            return ConnectionMultiplexer.Connect($"{primary.Value},{secondary.Value}");
        }

        private KeyVaultSecret GetSecret(SecretClient client, string key)
        {
            KeyVaultSecret secret;
            // Look for cache key.
            if (!_cache.TryGetValue(key, out secret))
            {
                Console.WriteLine($"Retrieving {key}");
                // Key not in cache, so get data.
                secret = client.GetSecret(key);

                // Keep in cache for only a limited time to allow worker to detect key rotation.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

                // Save data in cache.
                _cache.Set(key, secret, cacheEntryOptions);
                Console.WriteLine("Your secret is '" + secret.Value + "'.");
            }

            return secret;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {
                using (var connection = SetupConnection())
                {
                    var cache = connection.GetDatabase();

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

                    connection.Dispose();
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
