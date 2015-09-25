using System.Threading.Tasks;

namespace RedisRx
{
    public interface IDataProviderAsync<T>
    {
        Task<T> GetNext(string key);
    }
}