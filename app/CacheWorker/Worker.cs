using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheWorker
{
    public class Worker : BackgroundService
    {
        private readonly RedisConnectionFactory _factory;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IMemoryCache memoryCache)
        {
            _factory = new RedisConnectionFactory(logger, memoryCache);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                PerformCacheOperations();

                await Task.Delay(5000, stoppingToken);
            }
        }

        private void GetMessage(IDatabase cache)
        {
            _logger.LogInformation("Cache command  => {command} or StringGet()", "GET Message");
            _logger.LogInformation("Cache response <= {response}", cache.StringGet("Message").ToString());
        }

        private void PerformCacheOperations()
        {
            try
            {
                var connection = _factory.GetWorkingConnection();
                var cache = connection.GetDatabase();

                PingCache(cache);
                GetMessage(cache);
                PutMessage(cache);
                GetMessage(cache);
                ShowCacheClients(cache);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Cache Operations Error");
            }
        }

        private void PingCache(IDatabase cache)
        {
            const string cacheCommand = "PING";
            _logger.LogInformation("Cache command  => {command}", cacheCommand);
            _logger.LogInformation("Cache response <= {response}", cache.Execute(cacheCommand).ToString());
        }

        private void PutMessage(IDatabase cache)
        {
            var message = $"{DateTimeOffset.Now}: Hello! The cache is working from a .NET Core console app!";
            _logger.LogInformation("Cache command  => {command} or StringSet()", $"SET Message \"{message}\"");
            _logger.LogInformation("Cache response <= {response}", cache.StringSet("Message", message).ToString());
        }

        private void ShowCacheClients(IDatabase cache)
        {
            _logger.LogInformation("Get the client list, useful to see if connection list is growing...");
            _logger.LogInformation("Cache command  => {command}", "CLIENT LIST");
            _logger.LogInformation(
                "Cache response <= \n{response}",
                cache.Execute("CLIENT", "LIST").ToString()?.Replace("id=", "id="));
        }
    }
}