using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using RedisRx.Exceptions;
using StackExchange.Redis;

namespace RedisRx
{
    public class HashMapObvservableFactory : IObservableFactory<HashEntry[]>
    {
        private readonly IKeyspaceEventObservableFactory _notificationObservableFactory;
        private readonly IDataProviderAsync<HashEntry[]> _provider;
        private readonly ConcurrentDictionary<string, IObservable<HashEntry[]>> _cache = new ConcurrentDictionary<string, IObservable<HashEntry[]>>();

        public HashMapObvservableFactory(IDataProviderAsync<HashEntry[]> provider,
                                         IKeyspaceEventObservableFactory notificationObservableFactory)
        {
            _provider = provider;
            _notificationObservableFactory = notificationObservableFactory;
        }

        public IObservable<HashEntry[]> Create(string key)
        {
            return _cache.GetOrAdd(key, k =>
            {
                var observable = Observable.Create<HashEntry[]>(o =>
                {
                    var keyspaceMsgs = _notificationObservableFactory.Create(key);
                    var mutex = new SemaphoreSlim(1);
                    var subscription = keyspaceMsgs
                        .Where(ksn => ksn == KeyspaceEventType.MSET || ksn == KeyspaceEventType.HSET)
                        .Subscribe(async x =>
                        {
                            HashEntry[] result;
                            try
                            {
                                await mutex.WaitAsync();
                                result = await _provider.GetNext(key);
                            }
                            finally
                            {
                                mutex.Release();
                            }
                            o.OnNext(result);
                        });

                    var expiries = keyspaceMsgs
                            .Where(s => s == KeyspaceEventType.Expired)
                            .Subscribe(s => o.OnError(new KeyExpiredException(key)));

                    return Disposable.Create(() =>
                    {
                        IObservable<HashEntry[]> removed;
                        _cache.TryRemove(key, out removed);
                        expiries.Dispose();
                        subscription.Dispose();
                        mutex.Dispose();
                    });
                })
                .Replay(1)
                .RefCount();

                return observable;
            });
        }
    }
}