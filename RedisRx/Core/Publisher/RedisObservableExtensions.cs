using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisStreaming
{
    public abstract class RedisMapper<T>
    {
        public abstract RedisObject Do(T i);
    }

    public class SetMapper<T> : RedisMapper<IEnumerable<T>>
    {
        private ISerializer _serializer;

        public SetMapper<T> WithSerializer(ISerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public override RedisObject Do(IEnumerable<T> i)
        {
            return new RedisObject(RedisType.Set, i.Select(v => (RedisValue)JsonConvert.SerializeObject(v)).ToArray());
        }
    }

    public class HashMapMapper<T> : RedisMapper<T>
    {
        private ISerializer _serializer;

        public HashMapMapper<T> WithSerializer(ISerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        private class FieldMapping<T>
        {
            public Func<T, object> Func { get; set; }
            public string Key { get; set; }

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

        public override RedisObject Do(T invalue)
        {
            var length = _mappings.Count;
            HashEntry[] hes = new HashEntry[length];
            for (var i = 0; i < length; i++)
            {
                hes[i] = new HashEntry(_mappings[i].Key, JsonConvert.SerializeObject(_mappings[i].Func(invalue)));
            }
            return new RedisObject(RedisType.Hash, hes);
        }
    }


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