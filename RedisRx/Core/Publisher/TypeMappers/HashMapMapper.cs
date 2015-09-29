using System;
using System.Collections.Generic;
using RedisRx.Publisher.Types;
using RedisStreaming;
using StackExchange.Redis;

namespace RedisRx.Publisher.TypeMappers
{
    public class HashMapMapper<T> : RedisMapper<T>
    {
        private ISerializer _serializer = new JsonNetSerializer();

        public HashMapMapper<T> WithSerializer(ISerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        private class FieldMapping<T>
        {
            public Func<T, object> Func { get; private set; }
            public string Key { get; private set; }

            public FieldMapping(string key, Func<T, object> func)
            {
                Key = key;
                Func = func;
            }
        }

        private readonly List<FieldMapping<T>> _mappings = new List<FieldMapping<T>>();

        public HashMapMapper<T> WithField(string key, Func<T, object> func)
        {
            _mappings.Add(new FieldMapping<T>(key, func));
            return this;
        }

        public HashMapMapper<T> WithDefaults()
        {
            return this;
        }

        internal override RedisObject Do(T invalue)
        {
            var length = _mappings.Count;
            var values = new HashEntry[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = new HashEntry(_mappings[i].Key, _serializer.Serialize(_mappings[i].Func(invalue)));
            }
            return RedisObject.CreateHash(values);
        }
    }
}