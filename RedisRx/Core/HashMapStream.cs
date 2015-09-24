using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using StackExchange.Redis;

namespace Core
{
    public static class X
    {
        public static IObservable<TResult> CombineWithPrevious<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.Scan(Tuple.Create(default(TSource), default(TSource)),
                (previous, current) => Tuple.Create(previous.Item2, current))
                .Select(t => resultSelector(t.Item1, t.Item2));
        }
    }

    public class MockProvider : IHashMapProviderAsync
    {
        public Task<HashEntry[]> GetNext(Key k)
        {
            return Task.FromResult(new HashEntry[2]);
        }
    }

    public class HashEntryComparer
    {
        public HashEntry[] Compare(HashEntry[] prev, HashEntry[] current)
        {
            return current;
        }
    }

    public class KeySpaceNotificationBus
    {

    }

    public class NotificationBus : INotificationBus
    {
        private readonly ISubscriber _subscriber;

        public NotificationBus(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public Task PublishRequest(Key key)
        {
            return null;
        }

        public IObservable<string> Get(Key key)
        {
            return Observable.Create<string>(o =>
            {
                Action<RedisChannel, RedisValue> x = (channel, value) => { o.OnNext(value.ToString()); };
                _subscriber.Subscribe("", x);

                return Disposable.Create(async () =>
                {
                    await _subscriber.UnsubscribeAsync("", x);
                });
            });
        }
    }

    public static class NotificationType
    {
        public const string MSET = "mset";
        public const string HSET = "hset";
        public const string Expired = "expired";
    }

    public class HashMapSubProvider
    {
        private readonly Func<HashEntry[], HashEntry[], HashEntry[]> _diffFunc;
        public INotificationBus notificationBus;
        public IHashMapProviderAsync provider;
        private SingleThreadSynchronizationContext context = new SingleThreadSynchronizationContext();
        private NotificationBus _keySpaceNotificationProvider;

        public HashMapSubProvider(IDatabase db, ISubscriber subscriber)
        {
            _keySpaceNotificationProvider = new NotificationBus(subscriber);
            var comparer = new HashEntryComparer();
            _diffFunc = comparer.Compare;
            provider = new MockProvider();
        }

        private readonly ConcurrentDictionary<Key, IObservable<HashMapMessage>> _cache = new ConcurrentDictionary<Key, IObservable<HashMapMessage>>();
        public IObservable<HashMapMessage> HashMap(Key key)
        {
            return _cache.GetOrAdd(key, k =>
            {
                var mutex = new SemaphoreSlim(1);
                var keyspaceMsgs = _keySpaceNotificationProvider.Get(key);

                var observable = keyspaceMsgs
                    .Where(ksn => ksn == NotificationType.MSET || ksn == NotificationType.HSET)
                    .ObserveOn(context)
                    .SelectMany(async x =>
                    {
                        await mutex.WaitAsync();
                        var next = await provider.GetNext(key);
                        mutex.Release();
                        return next;
                    })
                    .DistinctUntilChanged() //add comparer
                    .Select(s => new HashMapMessage() { Entry = s });

                return Observable.Create<HashMapMessage>(async o =>
                {
                    await _keySpaceNotificationProvider.PublishRequest(key);
                    var expiries = keyspaceMsgs.Where(s => s == NotificationType.Expired).Subscribe(s => o.OnError(new Exception()));
                    var d = observable.Subscribe(o);

                    return Disposable.Create(() =>
                    {
                        IObservable<HashMapMessage> removed;
                        _cache.TryRemove(key, out removed);
                        expiries.Dispose();
                        d.Dispose();
                        mutex.Dispose();
                    });
                })
                    .Replay(1)
                    .RefCount();
            });
        }
    }

    public class HashMapStream : IObservable<Dictionary<RedisKey, RedisValue>>
    {
        public Key Key { get; private set; }
        private readonly IHashMapProviderAsync _provider;
        private List<IObserver<Dictionary<RedisKey, RedisValue>>> _observers = new List<IObserver<Dictionary<RedisKey, RedisValue>>>();
        private Dictionary<RedisKey, RedisValue> _last = null;
        public event Action Dispose;

        public HashMapStream(Key key, IHashMapProviderAsync provider)
        {
            _provider = provider;
            Key = key;
        }

        public async void GetNext()
        {
            //var raw = await _provider.GetNext(Key);
            //Dictionary<RedisKey, RedisValue> 

            //foreach (var observer  in _observers)
            //{
            //    observer.OnNext(diff);      
            //}
            //_last = raw;
        }

        public IDisposable Subscribe(IObserver<Dictionary<RedisKey, RedisValue>> observer)
        {
            _observers.Add(observer);
            if (_last != null)
            {
                observer.OnNext(_last);
            }
            return Disposable.Create(() =>
            {
                _observers.Remove(observer);
            });
        }
    }
}