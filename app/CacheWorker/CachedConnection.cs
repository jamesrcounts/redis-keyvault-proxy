using StackExchange.Redis;

namespace CacheWorker
{
    public class CachedConnection
    {
        public ConnectionMultiplexer Connection { get; private set; }
        public string Id { get; private set; }

        public CachedConnection(string id, string connectionString)
        {
            Id = id;
            Connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public bool IsValid()
        {
            try
            {
                // Test connection with PING
                return Connection.GetDatabase().Execute("PING").ToString() == "PONG";
            }
            catch
            {
                // PING failed
                return false;
            }
        }
    }
}