using StackExchange.Redis;

namespace RedisRx.Interfaces
{
    interface ISubscriptionRequestor
    {
        void Request(string key);
    }

    public class SubscripionRequestor : ISubscriptionRequestor
    {
        private readonly ISubscriber _subscriber;

        public SubscripionRequestor(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public void Request(string key)
        {
            //var value = "__redisrxrequests__:" + key;
            var channel = "SubscribeKey-" + key;
            _subscriber.PublishAsync(channel, key);
        }
    }
}
