using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using RedisRx.Exceptions;
using RedisRx.Interfaces;

namespace RedisRx
{
    public class ObservableFactory<T> : IObservableFactory<T>
    {
        private readonly IObservableFactory<string> _notificationObservableFactory;
        private readonly ISubscriptionRequestor _s;
        private readonly HashSet<string> _updateOn;
        private readonly Func<T, bool> _valueIsNull;
        private readonly IDataProviderAsync<T> _dataProvider;
        private readonly ConcurrentDictionary<string, IObservable<T>> _cache = new ConcurrentDictionary<string, IObservable<T>>();

        internal ObservableFactory(IDataProviderAsync<T> dataProvider,
            IObservableFactory<string> notificationObservableFactory,
            ISubscriptionRequestor s,
            HashSet<string> updateOn, Func<T, bool> valueIsNull = null)
        {
            _dataProvider = dataProvider;
            _notificationObservableFactory = notificationObservableFactory;
            _s = s;
            _updateOn = updateOn;

            if (valueIsNull != null)
            {
                _valueIsNull = valueIsNull;
            }
            else
            {
                _valueIsNull = t => false;
            }
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
                _s.Request(k);
                var observable = Observable.Create<T>(async o =>
                {
                    var keyspaceMsgs = _notificationObservableFactory.Create(key);
                    var mutex = new SemaphoreSlim(1);
                    var subscription = keyspaceMsgs
                        .Where(ksn => _updateOn.Contains(ksn))
                        .Subscribe(async x =>
                        {
                            try
                            {
                                var result = await GetNext(mutex, key);
                                o.OnNext(result);
                            }
                            catch (Exception e)
                            {
                                o.OnError(e);
                            }
                        });

                    var expiries = keyspaceMsgs
                        .Where(s => s == KeyspaceEvents.Expired)
                        .Subscribe(s => o.OnError(new KeyExpiredException(key)));

                    //initial value
                    GetNext(mutex, key).ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                        {
                            o.OnNext(t.Result);
                        }
                        else
                        {
                            o.OnError(t.Exception);
                        }
                    });

                    return Disposable.Create(() =>
                    {
                        IObservable<T> removed;
                        _cache.TryRemove(key, out removed);
                        expiries.Dispose();
                        subscription.Dispose();
                        mutex.Dispose();
                    });
                })
                    .Where((t) => !_valueIsNull(t))
                    .Replay(1)
                    .RefCount();

                return observable;
            });
        }
    }
}