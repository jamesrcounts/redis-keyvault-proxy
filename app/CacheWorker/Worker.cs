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
        private readonly ILogger<Worker> _logger;
        private readonly RedisConnectionFactory _factory;

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

        private void PerformCacheOperations()
        {
            try
            {
                var connection = _factory.GetWorkingConnection();
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
