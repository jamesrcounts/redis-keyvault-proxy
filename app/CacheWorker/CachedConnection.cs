using StackExchange.Redis;

namespace CacheWorker
{
    public class CachedConnection
    {
        public ConnectionMultiplexer Connection { get; set; }
        public int TTL { get; set; } = 10;
        public string Instance { get; set; }
    }
}