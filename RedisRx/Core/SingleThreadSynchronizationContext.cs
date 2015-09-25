using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RedisRx
{
    public sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public SingleThreadSynchronizationContext()
        {
            Thread t = new Thread((_) =>
            {
                Console.WriteLine("Thread in sc - " + Thread.CurrentThread.ManagedThreadId);
                KeyValuePair<SendOrPostCallback, object> workItem;
                while (m_queue.TryTake(out workItem, Timeout.Infinite))
                {
                    workItem.Key(workItem.Value);
                }
            });
            t.Start();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        // public void Complete() { m_queue.CompleteAdding(); }
    }
}