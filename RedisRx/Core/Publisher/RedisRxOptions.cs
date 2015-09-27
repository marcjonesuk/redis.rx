using System;

namespace RedisStreaming
{
    public class PublishOptions
    {
        public TimeSpan Expiry { get; set; }
        public bool DeleteOnDispose { get; set; }
    }
}