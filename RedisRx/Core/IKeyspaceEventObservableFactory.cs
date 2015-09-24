using System;

namespace Core
{
    public interface IKeyspaceEventObservableFactory
    {
        IObservable<string> Create(string key);
    }
}