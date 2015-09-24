using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Core
{
    public abstract class Key
    {

    }

    public interface IHashMapProviderAsync
    {
        Task<HashEntry[]> GetNext(Key key);
    }

    public interface INotificationBus
    {
        IObservable<string> Get(Key key);
    }

    public class HashMapMessage
    {
        public HashEntry[] Entry { get; set; }
    }

    public interface IHashMapSubscription
    {
        Key Key { get; }
        event Action<HashMapMessage> Updated;
    }

    public interface IPerformanceMonitor
    {
        void OnKeySpaceNotification();
    }

    public class IWorker
    {

    }


    public class Update
    {
        public Key Key { get; private set; }
        public Update(Key key)
        {
            Key = key;
        }
    }

    public delegate void Processor(Update nextItem);

    public class StreamProvider
    {
        private readonly IPerformanceMonitor _perfmon;
        private readonly ISerializer _serializer;
        private readonly IHashMapProviderAsync _hashMapProvider;
        private readonly INotificationBus _notificationBus;
        private ConcurrentDictionary<Key, object> _subscribers = new ConcurrentDictionary<Key, object>();

        public StreamProvider(IPerformanceMonitor perfmon, ISerializer serializer, IHashMapProviderAsync hashMapProvider, INotificationBus notificationBus)
        {
            _perfmon = perfmon;
            _serializer = serializer;
            _hashMapProvider = hashMapProvider;
            _notificationBus = notificationBus;

        }

        public IObservable<HashEntry[]> HashMap(Key key)
        {
            return (IObservable<HashEntry[]>)_subscribers.GetOrAdd(key, k =>
            {
                var stream = new HashMapStream(key, _hashMapProvider);
                stream.Dispose += () =>
                {
                    object s;
                    _subscribers.TryRemove(key, out s);
                };
                //_notificationBus.Subscribe(key1 => { stream.GetNext(); });
                return stream;
            });
        }
    }
}
