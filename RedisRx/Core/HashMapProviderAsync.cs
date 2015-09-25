using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRx
{
    public class HashMapDataProviderAsync : IDataProviderAsync<HashEntry[]>
    {
        private readonly IDatabase _database;

        public HashMapDataProviderAsync(IDatabase database)
        {
            _database = database;
        }

        public Task<HashEntry[]> GetNext(string key)
        {
            return _database.HashGetAllAsync(key);
        }
    }
}