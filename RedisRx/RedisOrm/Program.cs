using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;

namespace RedisOrm
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
         
            HashMapSubProvider hmp = new HashMapSubProvider();

            hmp.HashMap(null).Subscribe(x =>
            {
                Console.WriteLine(x);
            });

            Console.ReadLine();
        }
    }
}
