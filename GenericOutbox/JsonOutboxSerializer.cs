using System.Text.Json;

namespace GenericOutbox;

class JsonOutboxSerializer : ISerializer
{
    public bool IsBinary => false;

    public T? Deserialize<T>(byte[] data)
        => JsonSerializer.Deserialize<T>(data);

    public byte[] Serialize<T>(T? obj)
    {
        var ms = new MemoryStream();
        JsonSerializer.Serialize(ms, obj);
        return ms.ToArray();
    }
}