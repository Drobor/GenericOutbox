namespace GenericOutbox;

public interface ISerializer
{
    bool IsBinary { get; }
    T? Deserialize<T>(byte[] data);
    byte[] Serialize<T>(T? obj);
}