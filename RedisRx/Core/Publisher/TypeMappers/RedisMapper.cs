using RedisRx.Publisher.Types;

namespace RedisRx.Publisher.TypeMappers
{
    public abstract class RedisMapper<T>
    {
        internal abstract RedisObject Do(T i);
    }
}