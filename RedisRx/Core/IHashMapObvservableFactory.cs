using System;
using StackExchange.Redis;

namespace Core
{
    public interface IObservableFactory<out T>
    {
        IObservable<T> Create(string key);
    }
}