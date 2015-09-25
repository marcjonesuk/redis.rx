using System;

namespace RedisRx.Exceptions
{
    public class KeyDeletedException : Exception
    {
        public string Key { get; private set; }
        public KeyDeletedException(string key)
        {
            Key = key;
        }
    }
}
