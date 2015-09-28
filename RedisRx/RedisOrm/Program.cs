﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using RedisRx;
using RedisStreaming;
using StackExchange.Redis;

namespace RedisOrm
{
    public class Test
    {
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }

    o




    class Program
    {
        static void Main(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("localhost"); //D2APDEV001
            var redisRx = new RedisRxProvider(redis.GetDatabase(), redis.GetSubscriber());

            var options = new PublishOptions { DeleteOnDispose = true, Expiry = TimeSpan.FromSeconds(30) };

            redisRx.RedisPublish("test:*", (k) =>
            {
                var length = long.Parse(k.Split(':')[1]);
                return Observable.Interval(TimeSpan.FromSeconds(length));
            })
            .Wait();

            redisRx.RedisPublish("testmapxyz:*", (k) => /* respond to any requests matching string */
            {
                return Observable.Interval(TimeSpan.FromMilliseconds(250))
                    //.Select(x => new Test() { Bid = 100, Ask = 105 })
                    .Select(x => new HashSet<long>())
                    .AsRedisType(Map.ToSet());   

                    /* default convention based mapper */
            })
            .Wait();




            redisRx.RedisPublish("testmap:*", (k) => /* respond to any requests matching string */
                Observable.Interval(TimeSpan.FromMilliseconds(250))
                    .Select(x => new Test() { Bid = 100, Ask = 105 })
                    .AsRedisType(Map.ToHashMap<Test>()
                        .With("bid", t => t.Bid)
                        .With("ask", t => t.Ask))
                    .Sample(TimeSpan.FromSeconds(1)))
            .Wait();
            
            
            
            
            
            



            redisRx.HashMaps("key1").Subscribe((x) =>
            {

            });



            //redisRx.RedisPublish("hashmap:*", (k) =>
            //{
            //    return Observable.Interval(TimeSpan.FromMilliseconds(250))
            //        .Select(x =>
            //        {
            //            var d = new Dictionary<long, long>();
            //            d.Add(x, x + 1);
            //            return d;
            //        })
            //        .Sample(TimeSpan.FromSeconds(1));
            //}).Wait();





            redisRx.HashMaps("testmap:1").Subscribe(x =>
            {
                foreach (var v in x)
                {
                    Console.Write(v);
                }
                Console.WriteLine();
            });




            //redisRx.HashMaps("hashmap:2").Subscribe(x =>
            //{
            //    foreach (var v in x)
            //    {
            //        Console.Write(v);
            //    }
            //    Console.WriteLine();
            //});

            ////redisRx.Observable<long>("test:1").Subscribe(u => Console.WriteLine(u), e => Console.WriteLine(e));

            Console.ReadLine();
        }
    }
}