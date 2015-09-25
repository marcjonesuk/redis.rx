using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRx.DataProviders
{
    public class ListDataProviderAsync : IDataProviderAsync<RedisValue[]>
    {
        private readonly IDatabase _database;

        public ListDataProviderAsync(IDatabase database)
        {
            _database = database;
        }

        public Task<RedisValue[]> GetNext(string key)
        {
            return _database.ListRangeAsync(key);
        }
    }
}
