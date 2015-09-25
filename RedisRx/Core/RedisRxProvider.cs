using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace RedisRx
{
    public class RedisRxProvider
    {
        private readonly IKeyspaceEventObservableFactory _keyspaceEventObservableFactory;
        private readonly IObservableFactory<HashEntry[]> _hashMapObservableFactory;
        private readonly IObservableFactory<RedisValue> _stringObservableFactory;
        
        public RedisRxProvider(IDatabase db, ISubscriber sub)
        {
            _keyspaceEventObservableFactory = new KeyspaceEventObservableFactory(sub);       
            
            _hashMapObservableFactory = new ObservableFactory<HashEntry[]>(new HashMapDataProviderAsync(db), 
                _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.HSET, KeyspaceEventType.MSET });

            _stringObservableFactory = new ObservableFactory<RedisValue>(new StringProviderAsync(db),
                _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.HSET, KeyspaceEventType.MSET });
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
    }
}
