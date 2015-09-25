using System;

namespace RedisRx.Exceptions
{
    public class KeyExpiredException : Exception
    {
        public string Key { get; private set; }
        public KeyExpiredException(string key)
        {
            Key = key;
        }
    }
}
