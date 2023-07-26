using System.Collections.Concurrent;

namespace IntegrationTests.Services;

public class KeyStorageService : IKeyStorageService
{
    private static readonly ConcurrentDictionary<string, object> s_storageDictionary = new ConcurrentDictionary<string, object>();

    public bool Add(string key)
        => s_storageDictionary.TryAdd(key, null);

    public bool Contains(string key)
        => s_storageDictionary.ContainsKey(key);

    public void ThrowException()
        => throw new Exception();
}