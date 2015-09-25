using System;
using System.Collections.Generic;
using RedisRx.DataProviders;
using StackExchange.Redis;

namespace RedisRx
{
    public class RedisRxProvider
    {
        private readonly IObservableFactory<string> _keyspaceEventObservableFactory;
        private readonly IObservableFactory<HashEntry[]> _hashMapObservableFactory;
        private readonly IObservableFactory<RedisValue> _stringObservableFactory;
        private readonly IObservableFactory<RedisValue[]> _listObservableFactory;
        
        public RedisRxProvider(IDatabase db, ISubscriber sub)
        {
            _keyspaceEventObservableFactory = new KeyspaceEventObservableFactory(sub);       
            
            _hashMapObservableFactory = new ObservableFactory<HashEntry[]>(new HashMapProviderAsync(db),
                _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.HSet, KeyspaceEventType.MSet }, hashmap => hashmap.Length == 0);

            _stringObservableFactory = new ObservableFactory<RedisValue>(new StringProviderAsync(db),
                _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.Set });

            _listObservableFactory = new ObservableFactory<RedisValue[]>(new ListDataProviderAsync(db), 
              _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.HSet, KeyspaceEventType.MSet });
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

        public IObservable<RedisValue[]> Lists(string key)
        {
            return _listObservableFactory.Create(key);
        }
    }
}
