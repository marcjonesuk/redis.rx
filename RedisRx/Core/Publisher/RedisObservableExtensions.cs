using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisStreaming
{
    public class Mapping<T>
    {
        public Func<T, object> Func { get; private set; }
        public string Key { get; private set; }

        public Mapping(string key, Func<T, object> func)
        {
            Key = key;
            Func = func;
        }
    }

    public static class RedisObservableExtensions
    {
        public static IObservable<HashEntry[]> AsHashMap<T>(this IObservable<T> source, Mapping<T>[] mapping = null)
        {
            if (mapping == null)
            {
                mapping = CreateDefaultMapping<T>();
            }

            return source.Select(v =>
            {
                HashEntry[] hashmap = new HashEntry[mapping.Length];
                var c = 0;
                foreach (var m in mapping)
                {
                    var result = m.Func(v);
                    hashmap[c] = new HashEntry(JsonConvert.SerializeObject(m.Key), JsonConvert.SerializeObject(result));
                    c++;
                }
                return hashmap;
            });
        }

        private static Mapping<T>[] CreateDefaultMapping<T>()
        {
            return (new List<Mapping<T>>()).ToArray();
        }

        private static readonly ConcurrentDictionary<Type, MethodInfo> _typeCooercionLookup = new ConcurrentDictionary<Type, MethodInfo>();

        public static void ToRedis<T>(this IObservable<T> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            if (typeof(T) == typeof(HashEntry[]))
            {
                WriteAsHashMap((IObservable<HashEntry[]>)source, key, db, serializer);
                return;
            }

            var mi = _typeCooercionLookup.GetOrAdd(typeof(T), t =>
            {
             

                var interfaces = typeof(T).GetInterfaces();
                bool isBar = interfaces.Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                if (isBar)
                {
                    var genargs = typeof(T).GetGenericArguments();
                    MethodInfo method = typeof(RedisObservableExtensions).GetMethod("WriteAsHashMap", BindingFlags.Static | BindingFlags.NonPublic);
                    return method.MakeGenericMethod(genargs);
                };

                throw new Exception("sdfsfd");
            });
            mi.Invoke(null, new object[] { source, key, db, serializer });
        }

        private static void ToRedisString<T>(this IObservable<T> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Subscribe(async u =>
            {
                var s = JsonConvert.SerializeObject(u);
                await db.StringSetAsync(key, s);
            });
        }

        private static void WriteAsHashMap(this IObservable<HashEntry[]> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Subscribe(async u =>
            {
                await db.HashSetAsync(key, u);
            });
        }

        private static void WriteAsHashMap<TKey, TValue>(this IObservable<IDictionary<TKey, TValue>> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Subscribe(async u =>
            {
                HashEntry[] hashEntries = new HashEntry[u.Count];
                var c = 0;
                foreach (var value in u)
                {
                    hashEntries[c] = new HashEntry(JsonConvert.SerializeObject(value.Key), JsonConvert.SerializeObject(value.Value));
                    c++;
                }
                await db.HashSetAsync(key, hashEntries);
            });
        }

        private static void ToSet<T>(this IObservable<ISet<T>> source, string key, IDatabaseAsync db, ISerializer serializer = null)
        {
            source.Subscribe(async u =>
            {
                RedisValue[] set = new RedisValue[u.Count];
                var c = 0;
                foreach (var value in u)
                {
                    set[c] = JsonConvert.SerializeObject(value);
                    c++;
                }
                await db.SetAddAsync(key, set);
            });
        }
    }
}