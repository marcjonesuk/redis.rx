using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RedisRx;
using StackExchange.Redis;

namespace RedisOrm
{
    class Program
    {
        static void Main(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("localhost"); //D2APDEV001
            //var ksnObservableFactory = new KeyspaceEventTypeObservableFactory(redis.GetSubscriber());
            //var hashmapProvider = new DataProviderAsync(redis.GetDatabase());

            var redisrx = new RedisObservable(redis.GetDatabase(), redis.GetSubscriber());

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            for (var i = 0; i < 10000; i++)
            {
                int i1 = i % 500;
                redisrx.HashMaps("te3st:" + i1).Subscribe((x) =>
                {
                    
                    Console.WriteLine(x.Length);
                });
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            //Task.Run(() =>
            //{
            //    int x = 0;
            //    while (true)
            //    {
            //        Interlocked.Increment(ref x);
            //        for (var i = 0; i < 10; i++)
            //        {
            //            redis.GetDatabase().HashSetAsync("test:" + i, new HashEntry[] { new HashEntry("name", x) });
            //        }
            //        Thread.Sleep(250);
            //    }
            //});

            Console.ReadLine();
        }
    }
}
