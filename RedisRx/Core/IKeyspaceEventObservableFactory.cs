using System;

namespace RedisRx
{
    public interface IKeyspaceEventObservableFactory
    {
        IObservable<string> Create(string key);
    }
}