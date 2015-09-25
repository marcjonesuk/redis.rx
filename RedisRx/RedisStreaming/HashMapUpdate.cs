﻿using System.Collections.Generic;
using StackExchange.Redis;

namespace RedisStreaming
{
    public class HashMapUpdate
    {
        public IDictionary<RedisValue, RedisValue> Value { get; private set; }

        private ISet<RedisValue> _updatedKeys;
        public ISet<RedisValue> UpdatedKeys
        {
            get
            {
                _updatedKeys = UpdateComparer.GetUpdatedKeys(Value, Previous != null ? Previous.Value : null);
                return _updatedKeys;
            }
        }

        internal HashMapUpdate Previous { get; set; }
        internal HashMapUpdate(Dictionary<RedisValue, RedisValue> value)
        {
            Value = value;
        }
    }
}
