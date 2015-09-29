using StackExchange.Redis;

namespace RedisStreaming
{
    public class RedisObject
    {
        public RedisType RedisType { get; set; }
        public HashEntry[] Hash { get; set; }
        public RedisValue[] List { get; set; }

        public RedisValue[] Added { get; set; }
        public RedisValue[] Removed { get; set; }

        public RedisObject(RedisType t, HashEntry[] hash)
        {
            RedisType = t;
            Hash = hash;
        }

        public RedisObject(RedisType t, RedisValue[] list)
        {
            RedisType = t;
            List = list;
        }
    }



    public class RedisHashMap
    {
        public HashEntry[] Value { get; private set; }
        public RedisHashMap(HashEntry[] value)
        {
            Value = value;
        }
    }
}