using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using StackExchange.Redis;

namespace RedisRx
{
    public class SubscriptionObservableFactory : IObservableFactory<string>
    {
        private readonly ISubscriber _subscriber;

        public SubscriptionObservableFactory(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public IObservable<string> Create(string key)
        {
            return Observable.Create<string>(async o =>
            {
                //var channel = "__redisrxrequests__:" + key;
                var channel = "SubscribeKey-" + key;
                Action<RedisChannel, RedisValue> handler = (c, value) => { o.OnNext(value.ToString()); };
                await _subscriber.SubscribeAsync(channel, handler);
                o.OnNext(null);

                return Disposable.Create(async () =>
                {
                    await _subscriber.UnsubscribeAsync(channel, handler);  //need to test when this is called if two subscribe
                });
            });
        }
    }
}