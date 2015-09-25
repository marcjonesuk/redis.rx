using System.Collections.Generic;
using StackExchange.Redis;

namespace RedisStreaming
{
    public class UpdateComparer
    {
        public static ISet<RedisValue> GetUpdatedKeys(IDictionary<RedisValue, RedisValue> current,
            IDictionary<RedisValue, RedisValue> previous)
        {
            var updatedKeys = new HashSet<RedisValue>();
            foreach (var pair in current)
            {
                if (previous == null || !previous.ContainsKey(pair.Key) || previous[pair.Key] != pair.Value)
                {
                    updatedKeys.Add(pair.Key);
                }
            }
            return updatedKeys;
        }
    }
}