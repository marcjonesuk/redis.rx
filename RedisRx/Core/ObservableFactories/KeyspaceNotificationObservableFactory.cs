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
        private readonly ConcurrentDictionary<string, IObservable<string>> _cache = new ConcurrentDictionary<string, IObservable<string>>();

        public KeyspaceEventObservableFactory(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public IObservable<string> Create(string key)
        {
            return _cache.GetOrAdd(key, k =>
            {
                return Observable.Create<string>(o =>
                {
                    Action<RedisChannel, RedisValue> x = (channel, value) => { o.OnNext(value.ToString()); };
                    _subscriber.SubscribeAsync("__keyspace@0__:" + key, x);
                    return Disposable.Create(async () =>
                    {
                        await _subscriber.UnsubscribeAsync("", x);
                    });
                });
            });
        }
    }
}