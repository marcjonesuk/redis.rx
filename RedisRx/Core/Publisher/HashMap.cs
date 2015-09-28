using StackExchange.Redis;

namespace RedisStreaming
{
    public class HashMap
    {
        public HashEntry[] Value { get; private set; }
        public HashMap(HashEntry[] value)
        {
            Value = value;
        }
    }

    public class RedisHashSet
    {
        public HashEntry[] Value { get; private set; }
        public RedisHashSet(HashEntry[] value)
        {
            Value = value;
        }
    }
}