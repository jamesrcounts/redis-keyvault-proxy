using StackExchange.Redis;

namespace CacheWorker
{
    public class CachedConnection
    {
        public ConnectionMultiplexer Connection { get; set; }
        public string Instance { get; set; }
        public int TTL { get; set; } = 10;
    }
}
