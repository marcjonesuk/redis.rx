using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRx
{
    public class HashMapProviderAsync : IDataProviderAsync<HashEntry[]>
    {
        private readonly IDatabaseAsync _database;

        public HashMapProviderAsync(IDatabaseAsync database)
        {
            _database = database;
        }

        public Task<HashEntry[]> GetNext(string key)
        {
            return _database.HashGetAllAsync(key);
        }
    }
}