using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Core
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

    public class StringProviderAsync : IDataProviderAsync<string>
    {
        private readonly IDatabase _database;

        public StringProviderAsync(IDatabase database)
        {
            _database = database;
        }

        public Task<string> GetNext(string key)
        {
            return _database.StringGetAsync(key).ContinueWith((t) => (string) t.Result);
        }
    }
}