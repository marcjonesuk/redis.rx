using StackExchange.Redis;

namespace RedisStreaming
{
    public class RedisHashSet
    {
        public RedisValue[] Value { get; private set; }
        public RedisHashSet(RedisValue[] value)
        {
            Value = value;
        }

        public RedisValue[] Added;
        public RedisValue[] Removed;
    }
}