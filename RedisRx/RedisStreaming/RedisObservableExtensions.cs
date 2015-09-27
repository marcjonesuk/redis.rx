//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;
//using StackExchange.Redis;

//namespace RedisStreaming
//{
//    public static class RedisRxExtensions
//    {
//        public static void ToRedis<T>(this IObservable<T> source, string key, IDatabaseAsync db, ISerializer serializer = null)
//        {
//            source.Subscribe(async u =>
//            {
//                var s = JsonConvert.SerializeObject(u);
//                await db.StringSetAsync(key, s);
//            });
//        }

//        public static void ToRedis<TKey, TValue>(this IObservable<IDictionary<TKey, TValue>> source, string key, IDatabaseAsync db, ISerializer serializer = null)
//        {
//            source.Subscribe(async u =>
//            {
//                HashEntry[] hashEntries = new HashEntry[u.Count];
//                var c = 0;
//                foreach (var value in u)
//                {
//                    hashEntries[c] = new HashEntry(JsonConvert.SerializeObject(value.Key), JsonConvert.SerializeObject(value.Value));
//                }
//                await db.HashSetAsync(key, hashEntries);
//            });
//        }
//    }
//}