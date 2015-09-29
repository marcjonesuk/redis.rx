using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RedisRx.Publisher.TypeMappers;
using RedisRx.Publisher.Types;
using StackExchange.Redis;

namespace RedisRx.Publisher
{
    public static class RedisObservableExtensions
    {
        public static IObservable<RedisObject> AsRedisHash<T>(this IObservable<T> source, Action<HashMapMapper<T>> func = null)
        {
            var mapper = new HashMapMapper<T>();

            if (func != null)
                func(mapper);

            return source.Select(mapper.Do);
        }

        public static IObservable<RedisObject> AsRedisSet<T>(this IObservable<IEnumerable<T>> source, Action<SetMapper<T>> func = null)
        {
            var mapper = new SetMapper<T>();

            if (func != null)
                func(mapper);

            return source.Select(mapper.Do);
        }

        internal static void ToRedis<T>(this IObservable<T> source, string key, IDatabaseAsync db, ISerializer serializer = null) where T : RedisObject
        {
            Func<RedisObject, Task> saveFunc = null;

            source.Subscribe(async v =>
            {
                if (saveFunc == null)
                {
                    switch (v.RedisType)
                    {
                        case RedisType.Hash:
                            saveFunc = async (x) => { await db.HashSetAsync(key, x.Hash); };
                            break;
                        case RedisType.Set:
                            saveFunc = async (x) => { await db.HashSetAsync(key, x.Hash); };
                            break;

                        default:
                            throw new Exception();
                    }
                }
                await saveFunc(v);
            });
        }

        internal static void ToRedisHash(this IObservable<RedisObject> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Subscribe(async u =>
            {
                await db.HashSetAsync(key, u.Hash);
            });
        }

        internal static void ToRedisSet(this IObservable<RedisObject> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Scan((prev, current) =>
            {
                current.Added = current.List.Except(prev.List).ToArray();
                current.Removed = prev.List.Except(current.List).ToArray();
                return current;
            })
            .Subscribe(async u =>
            {
                await db.SetAddAsync(key, u.Added);
                await db.SetRemoveAsync(key, u.Removed);
            });
        }

    }
}