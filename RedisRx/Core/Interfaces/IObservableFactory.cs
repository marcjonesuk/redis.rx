using System;

namespace RedisRx
{
    public interface IObservableFactory<out T>
    {
        IObservable<T> Create(string key);
    }
}