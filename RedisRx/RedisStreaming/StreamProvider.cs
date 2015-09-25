using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using RedisRx;
using StackExchange.Redis;

namespace RedisStreaming
{
    public class StreamProvider
    {
        private readonly RedisRxProvider _redisRx;

        public StreamProvider(RedisRxProvider redisRx)
        {
            _redisRx = redisRx;
        }

        public IObservable<HashMapUpdate> HashMaps(string key)
        {
            return Observable.Create<HashMapUpdate>(o =>
            {
                var subscription = _redisRx.HashMaps(key)
                    .Select((hm) => new HashMapUpdate(hm.ToDictionary())) //add as immutable?
                    .Scan((last, current) =>
                    {
                        current.Previous = last;
                        return current;
                    })
                    .Where(update => update.UpdatedKeys.Count > 0)
                    .Subscribe(o.OnNext);

                return Disposable.Create(subscription.Dispose);
            })
                .Replay(1)
                .RefCount();
        }
    }
}