using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedisRx.Consumer.DataProviders;
using RedisRx.DataProviders;
using RedisRx.Interfaces;
using RedisRx.Publisher.Types;
using RedisStreaming;
using StackExchange.Redis;

namespace RedisRx
{
    public class RedisRxProvider
    {
        private readonly ISubscriber _sub;
        private readonly Publisher.Publisher _publisher;
        private readonly IObservableFactory<string> _keyspaceEventObservableFactory;
        private readonly IObservableFactory<HashEntry[]> _hashMapObservableFactory;
        private readonly IObservableFactory<RedisValue> _stringObservableFactory;
        private readonly IObservableFactory<RedisValue[]> _listObservableFactory;

        public RedisRxProvider(IDatabaseAsync db, ISubscriber sub)
        {
            _sub = sub;
            _publisher = new Publisher.Publisher(db, sub);
            var req = new SubscripionRequestor(sub);

            _keyspaceEventObservableFactory = new KeyspaceEventObservableFactory(sub);

            _hashMapObservableFactory = new ObservableFactory<HashEntry[]>(new HashMapProviderAsync(db),
                _keyspaceEventObservableFactory,
                req,
                new HashSet<string>() { KeyspaceEvents.HSet, KeyspaceEvents.MSet },
                hashmap => hashmap.Length == 0);

            _stringObservableFactory = new ObservableFactory<RedisValue>(new StringProviderAsync(db),
                _keyspaceEventObservableFactory,
                req,
                new HashSet<string>() { KeyspaceEvents.Set });

            _listObservableFactory = new ObservableFactory<RedisValue[]>(new ListDataProviderAsync(db),
              _keyspaceEventObservableFactory,
              req,
              new HashSet<string>() { KeyspaceEvents.HSet, KeyspaceEvents.MSet });
        }

        public IObservable<string> KeySpaceMessages(string key)
        {
            return _keyspaceEventObservableFactory.Create(key);
        }

        public IObservable<HashEntry[]> HashMaps(string key)
        {
            return _hashMapObservableFactory.Create(key);
        }

        public IObservable<RedisValue> Strings(string key)
        {
            return _stringObservableFactory.Create(key);
        }

        public IObservable<T> Observable<T>(string key)
        {
            return _stringObservableFactory.Create(key)
                .Where(s => !s.IsNull)
                .Select(s => JsonConvert.DeserializeObject<T>(s));
        }

        public IObservable<RedisValue[]> Lists(string key)
        {
            return _listObservableFactory.Create(key);
        }

        public Task Publish(string key, Func<string, IObservable<RedisObject>> handler)
        {
            return _publisher.AddHandler(key, handler, null);
        }

        public Task RedisPublish<T>(string key, PublishOptions options, Func<string, IObservable<RedisObject>> handler)
        {
            return _publisher.AddHandler(key, handler, options);
        }
    }
}
