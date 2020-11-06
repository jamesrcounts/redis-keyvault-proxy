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

        public Worker(ILogger<Worker> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
        }

        private CachedConnection SetupConnection(string instance)
        {
            try
            {
                _logger.LogInformation("Connecting to {instance} redis instance.", instance);

                string secretId = "redis-connection-string-primary";
                if (instance != "primary")
                {
                    secretId = "redis-connection-string-secondary";
                }

                KeyVaultSecret secret = GetSecret(secretId);
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(secret.Value);
                var cache = connection.GetDatabase();
                _logger.LogInformation("Cache connection response <= {response}", cache.Execute("PING").ToString());

                return new CachedConnection
                {
                    Instance = instance,
                    Connection = connection
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get working connection: {connection}", instance);
                return null;
            }
        }

        private KeyVaultSecret GetSecret(string key)
        {
            var client = new SecretClient(
                new Uri("https://kv-redis-keyvault-proxy.vault.azure.net/"),
                new DefaultAzureCredential());

            _logger.LogInformation($"Retrieving {key}");
            KeyVaultSecret secret = client.GetSecret(key);
            _logger.LogDebug("Your secret is '{secret}'.", secret.Value);
            return secret;
        }

        private ConnectionMultiplexer GetWorkingConnection()
        {
            const string cacheConnection = nameof(cacheConnection);
            const string primary = nameof(primary);
            const string secondary = nameof(secondary);

            if (!_cache.TryGetValue<CachedConnection>(cacheConnection, out var cached))
            {
                _logger.LogInformation("No cached connection");
                cached = SetupWorkingConnection(primary, secondary);
                PutConnection(cacheConnection, cached);
            }

            if (cached.TTL <= 0)
            {
                _cache.Remove(cacheConnection);
                cached = SetupWorkingConnection(primary, secondary);
                PutConnection(cacheConnection, cached);
            }

            cached.TTL -= 1;
            _logger.LogInformation("Connection TTL for {instance}: {ttl}", cached.Instance, cached.TTL);
            return cached.Connection;

            void PutConnection(string key, CachedConnection cached)
            {
                // Expire after 5 minutes even if TTL has not reached 0
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        var item = value as CachedConnection;
                        if (item != null)
                        {
                            _logger.LogInformation("Disposing of stale cache item (formerly {instance}).", item.Instance);
                            item.Connection.Dispose();
                        }
                    });

                _cache.Set(key, cached, cacheEntryOptions);
                _logger.LogInformation("Cached connection created for {instance}.", cached.Instance);
            }

            CachedConnection SetupWorkingConnection(string primary, string secondary)
            {
                return SetupConnection(primary) ??
                    SetupConnection(secondary) ??
                    throw new Exception("Could not get working Redis connection.");
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
                var connection = GetWorkingConnection();
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
