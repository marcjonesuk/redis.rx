using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Exceptions;

namespace Core
{
    public class ObservableFactory<T> : IObservableFactory<T>
    {
        private readonly IKeyspaceEventObservableFactory _notificationObservableFactory;
        private readonly HashSet<string> _updateOn;
        private readonly IDataProviderAsync<T> _dataProvider;
        private readonly ConcurrentDictionary<string, IObservable<T>> _cache = new ConcurrentDictionary<string, IObservable<T>>();

        internal ObservableFactory(IDataProviderAsync<T> dataProvider,
            IKeyspaceEventObservableFactory notificationObservableFactory,
            HashSet<string> updateOn)
        {
            _dataProvider = dataProvider;
            _notificationObservableFactory = notificationObservableFactory;
            _updateOn = updateOn;
        }

        private async Task<T> GetNext(SemaphoreSlim mutex, string key)
        {
            try
            {
                await mutex.WaitAsync();
                return await _dataProvider.GetNext(key);
            }
            finally
            {
                mutex.Release();
            }
        }

        public IObservable<T> Create(string key)
        {
            return _cache.GetOrAdd(key, k =>
            {
                var observable = Observable.Create<T>(async o =>
                {
                    var keyspaceMsgs = _notificationObservableFactory.Create(key);
                    var mutex = new SemaphoreSlim(1);
                    var subscription = keyspaceMsgs
                        .Where(ksn => _updateOn.Contains(ksn))
                        .Subscribe(async x =>
                        {
                            var result = await GetNext(mutex, key);
                            o.OnNext(result);
                        });

                    var expiries = keyspaceMsgs
                        .Where(s => s == KeyspaceEventType.Expired)
                        .Subscribe(s => o.OnError(new KeyExpiredException(key)));

                    //initial value
                    GetNext(mutex, key).ContinueWith(t => o.OnNext(t.Result));

                    return Disposable.Create(() =>
                    {
                        IObservable<T> removed;
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