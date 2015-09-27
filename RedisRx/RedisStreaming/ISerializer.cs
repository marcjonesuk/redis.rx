namespace RedisStreaming
{
    public interface ISerializer
    {
        string Serialize(object t);
    }
}