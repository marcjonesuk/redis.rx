using System;
using System.Linq;
using System.Reactive.Linq;
using StackExchange.Redis;

namespace RedisRx
{
    //public class ThrottlingKeyspaceObservableFactory : IKeyspaceEventObservableFactory
    //{
    //    private readonly IObservable<long> _timer;
    //    private readonly KeyspaceEventObservableFactory _wrapped;

    //    public ThrottlingKeyspaceObservableFactory(ISubscriber subscriber, TimeSpan length)
    //    {
    //        _wrapped = new KeyspaceEventObservableFactory(subscriber);
    //        _timer = Observable.Interval(length);
    //    }

    //    public IObservable<string> Create(string key)
    //    {
    //        var ksn = _wrapped.Create(key);
    //        var buffered = ksn.Skip(1).Where(k => k != "expired").Buffer(_timer);
    //        var first = ksn.Take(1);

    //        return ksn.Buffer(_timer)
    //            .Where(x => x.Any())
    //            .Select(x => x.Last());
    //    }
    //}
}