using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Core
{
    public class RedisRxProvider
    {
        private readonly IKeyspaceEventObservableFactory _keyspaceEventObservableFactory;
        private readonly IObservableFactory<HashEntry[]> _hashMapObservableFactory;
        private readonly IObservableFactory<string> _stringObservableFactory;
        
        public RedisRxProvider(IDatabase db, ISubscriber sub)
        {
            _keyspaceEventObservableFactory = new KeyspaceEventObservableFactory(sub);       
            
            _hashMapObservableFactory = new ObservableFactory<HashEntry[]>(new HashMapDataProviderAsync(db), 
                _keyspaceEventObservableFactory, new HashSet<string>() { KeyspaceEventType.HSET, KeyspaceEventType.MSET });

            _stringObservableFactory = new ObservableFactory<string>(new StringProviderAsync(db),
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

        public IObservable<string> Strings(string key)
        {
            return _stringObservableFactory.Create(key);
        }
    }
}
