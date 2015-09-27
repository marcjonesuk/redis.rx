using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRx
{
    public class StringProviderAsync : IDataProviderAsync<RedisValue>
    {
        private readonly IDatabaseAsync _database;

        public StringProviderAsync(IDatabaseAsync database)
        {
            _database = database;
        }

        public Task<RedisValue> GetNext(string key)
        {
            return _database.StringGetAsync(key);
        }
    }
}