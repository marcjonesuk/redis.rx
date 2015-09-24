using System.Threading.Tasks;
using StackExchange.Redis;

namespace Core
{
    public interface IDataProviderAsync<T>
    {
        Task<T> GetNext(string key);
    }
}