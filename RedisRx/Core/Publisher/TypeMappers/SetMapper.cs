using System.Collections.Generic;
using System.Linq;
using RedisRx.Publisher.Types;
using StackExchange.Redis;

namespace RedisRx.Publisher.TypeMappers
{
    public class SetMapper<T> : RedisMapper<IEnumerable<T>>
    {
        private ISerializer _serializer = new JsonNetSerializer();

        public SetMapper<T> WithSerializer(ISerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        internal override RedisObject Do(IEnumerable<T> i)
        {
            return RedisObject.CreateSet(i.Select(v => (RedisValue)_serializer.Serialize(v)).ToArray());
        }
    }
}