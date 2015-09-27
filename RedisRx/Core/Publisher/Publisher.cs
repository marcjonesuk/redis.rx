using System;
using System.Threading.Tasks;
using RedisStreaming;
using StackExchange.Redis;

namespace RedisRx.Publisher
{
    public class Publisher
    {
        private readonly IDatabaseAsync _db;
        private readonly IObservableFactory<string> _req;

        public Publisher(IDatabaseAsync db, ISubscriber subscriber)
        {
            _db = db;
            _req = new SubscriptionObservableFactory(subscriber);
        }

        public Task AddHandler<T>(string key, Func<string, IObservable<T>> handler, PublishOptions options)
        {
            var tcs = new TaskCompletionSource<object>();
            try
            {
                _req.Create(key).Subscribe(req =>
                {
                    try
                    {
                        if (req == null && !tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(null);
                            return;
                        }

                        var ob = handler(req);
                        if (ob != null)
                            ob.ToRedis(req, _db);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
            return tcs.Task;
        }
    }
}
