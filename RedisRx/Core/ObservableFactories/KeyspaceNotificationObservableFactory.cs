using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using StackExchange.Redis;

namespace RedisRx
{
    public class KeyspaceEventObservableFactory : IObservableFactory<string>
    {
        private readonly ISubscriber _subscriber;

        public KeyspaceEventObservableFactory(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public IObservable<string> Create(string key)
        {
            return Observable.Create<string>(o =>
            {
                var channel = "__keyspace@0__:" + key;
                Action<RedisChannel, RedisValue> x = (c, value) => { o.OnNext(value.ToString()); };
                _subscriber.SubscribeAsync(channel, x);
                return Disposable.Create(async () =>
                {
                    await _subscriber.UnsubscribeAsync(channel, x);  //need to test when this is called if two subscribe
                });
            });
        }
    }
}