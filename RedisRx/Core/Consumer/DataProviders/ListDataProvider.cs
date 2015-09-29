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
        private readonly IDatabaseAsync _database;

        public ListDataProviderAsync(IDatabaseAsync database)
        {
            _database = database;
        }

        public Task<RedisValue[]> GetNext(string key)
        {
            return _database.ListRangeAsync(key);
        }
    }
}
