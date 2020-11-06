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
    public class CachedConnection
    {
        public ConnectionMultiplexer Connection { get; set; }
        public int TTL { get; set; } = 10;
    }

    public class Worker : BackgroundService
    {
        private IMemoryCache _cache;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
        }

        private CachedConnection SetupConnection(string instance)
        {
            _logger.LogInformation("Connecting to {instance} redis instance.", instance);

            var client = new SecretClient(
                new Uri("https://kv-redis-keyvault-proxy.vault.azure.net/"),
                new DefaultAzureCredential());

            string secretId = "redis-connection-string-primary";
            if (instance != "primary")
            {
                secretId = "redis-connection-string-secondary";
            }

            KeyVaultSecret secret = GetSecret(client, secretId);
            return new CachedConnection
            {
                Connection = ConnectionMultiplexer.Connect(secret.Value)
            };
        }

        private KeyVaultSecret GetSecret(SecretClient client, string key)
        {
            KeyVaultSecret secret;
            // Look for cache key.
            if (!_cache.TryGetValue(key, out secret))
            {
                _logger.LogInformation($"Retrieving {key}");
                // Key not in cache, so get data.
                secret = client.GetSecret(key);

                // Keep in cache for only a limited time to allow worker to detect key rotation.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

                // Save data in cache.
                _cache.Set(key, secret, cacheEntryOptions);
                _logger.LogDebug("Your secret is '{}'.", secret.Value);
            }

            return secret;
        }

        private ConnectionMultiplexer GetConnection(string instance)
        {
            if (!_cache.TryGetValue<CachedConnection>(instance, out var cached))
            {
                _logger.LogInformation("No cached connection for {instance}", instance);
                cached = SetupConnection(instance);
                PutConnection(instance, cached);
            }

            if (cached.TTL <= 0)
            {
                _cache.Remove(instance);
                cached = SetupConnection(instance);
                PutConnection(instance, cached);
            }

            cached.TTL -= 1;
            _logger.LogInformation("Connection TTL: {ttl}", cached.TTL);
            return cached.Connection;

            void PutConnection(string instance, CachedConnection cached)
            {
                // Expire after 5 minutes even if TTL has not reached 0
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        var item = value as CachedConnection;
                        if (item != null)
                        {
                            _logger.LogInformation("Disposing of stale cache item (formerly {key}).", key);
                            item.Connection.Dispose();
                        }
                    });

                _cache.Set(instance, cached, cacheEntryOptions);
                _logger.LogInformation("Cached connection created for {instance}.", instance);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTimeOffset now = DateTimeOffset.Now;
                _logger.LogInformation("Worker running at: {time}", now);

                PerformCacheOperations();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void PerformCacheOperations()
        {
            try
            {
                var connection = GetConnection("primary");
                var cache = connection.GetDatabase();

                // Simple PING command
                string cacheCommand = "PING";
                _logger.LogInformation("Cache command  => {command}", cacheCommand);
                _logger.LogInformation("Cache response <= {response}", cache.Execute(cacheCommand).ToString());

                // Simple get and put of integral data types into the cache
                GetMessage(cache);
                DateTimeOffset now = DateTimeOffset.Now;
                string message = $"{now}: Hello! The cache is working from a .NET Core console app!";
                cacheCommand = $"SET Message \"{message}\"";
                _logger.LogInformation("Cache command  => {command} or StringSet()", cacheCommand);
                _logger.LogInformation("Cache response <= {response}", cache.StringSet("Message", message).ToString());

                // Demonstrate "SET Message" executed as expected...
                GetMessage(cache);

                _logger.LogInformation("Get the client list, useful to see if connection list is growing...");
                cacheCommand = "CLIENT LIST";
                _logger.LogInformation("Cache command  => {command}", cacheCommand);
                _logger.LogInformation("Cache response <= \n{response}", cache.Execute("CLIENT", "LIST").ToString().Replace("id=", "id="));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Cache Operations Error");
            }
        }

        private void GetMessage(IDatabase cache)
        {
            string cacheCommand = "GET Message";
            _logger.LogInformation("Cache command  => {command} or StringGet()", cacheCommand);
            _logger.LogInformation("Cache response <= {response}", cache.StringGet("Message").ToString());
        }
    }
}
