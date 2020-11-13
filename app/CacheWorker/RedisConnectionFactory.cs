using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace CacheWorker
{
    public class RedisConnectionFactory
    {
        private const string CacheConnection = nameof(CacheConnection);
        private const string Primary = nameof(Primary);
        private const string Secondary = nameof(Secondary);

        private readonly IMemoryCache _cache;
        private readonly ILogger<Worker> _logger;

        public RedisConnectionFactory(ILogger<Worker> logger, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _logger = logger;
        }

        public ConnectionMultiplexer GetWorkingConnection()
        {
            if (!_cache.TryGetValue<CachedConnection>(CacheConnection, out var cached))
            {
                _logger.LogInformation("No cached connection");
                cached = SetupWorkingConnection(Primary, Secondary);
                PutConnection(CacheConnection, cached);
            }

            if (cached.TTL <= 0)
            {
                _cache.Remove(CacheConnection);
                cached = SetupWorkingConnection(Primary, Secondary);
                PutConnection(CacheConnection, cached);
            }

            cached.TTL -= 1;
            _logger.LogInformation("Connection TTL for {instance}: {ttl}", cached.Instance, cached.TTL);
            return cached.Connection;
        }

        private KeyVaultSecret GetSecret(string key)
        {
            var client = new SecretClient(
                new Uri("https://kv-redis-keyvault-proxy.vault.azure.net/"),
                new DefaultAzureCredential(),
                new SecretClientOptions
                {
                    Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                    }
                });

            _logger.LogInformation($"Retrieving {key}");
            KeyVaultSecret secret = client.GetSecret(key);
            _logger.LogDebug("Your secret is '{secret}'.", secret.Value);
            return secret;
        }

        private void PutConnection(string key, CachedConnection cached)
        {
            // Expire after 5 minutes even if TTL has not reached 0
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .RegisterPostEvictionCallback((k, value, reason, state) =>
                {
                    if (!(value is CachedConnection item)) return;

                    _logger.LogInformation("Disposing of stale cache item (formerly {instance}).", item.Instance);
                    item.Connection.Dispose();
                });

            _cache.Set(key, cached, cacheEntryOptions);
            _logger.LogInformation("Cached connection created for {instance}.", cached.Instance);
        }

        private CachedConnection SetupConnection(string instance)
        {
            try
            {
                _logger.LogInformation("Connecting to {instance} redis instance.", instance);

                var secretId = instance == Primary ?
                    "redis-connection-string-primary" :
                    "redis-connection-string-secondary";

                var secret = GetSecret(secretId);
                var connection = ConnectionMultiplexer.Connect(secret.Value);

                // Test connection with PING
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
                // PING failed
                _logger.LogError(ex, "Could not get working connection: {connection}", instance);
                return null;
            }
        }

        private CachedConnection SetupWorkingConnection(string primary, string secondary)
        {
            return SetupConnection(primary) ??
                   SetupConnection(secondary) ??
                   throw new Exception("Could not get working Redis connection.");
        }
    }
}