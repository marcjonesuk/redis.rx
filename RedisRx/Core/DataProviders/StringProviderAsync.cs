using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRx
{
    public class StringProviderAsync : IDataProviderAsync<RedisValue>
    {
        private readonly IDatabase _database;

        public StringProviderAsync(IDatabase database)
        {
            _database = database;
        }

        public Task<RedisValue> GetNext(string key)
        {
            return _database.StringGetAsync(key);
        }
    }
}