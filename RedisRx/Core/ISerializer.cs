namespace RedisRx
{
    public interface ISerializer
    {
        string Serialize(object t);
    }
}