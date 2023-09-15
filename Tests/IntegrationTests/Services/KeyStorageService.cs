using System.Collections.Concurrent;

namespace IntegrationTests.Services;

public class KeyStorageService : IKeyStorageService
{
    private static readonly ConcurrentDictionary<string, object> s_storageDictionary = new ConcurrentDictionary<string, object>();

    private readonly Dictionary<string, string> _dictionary;

    public KeyStorageService(Dictionary<string, string> dictionary)
    {
        _dictionary = dictionary;
    }

    public bool Add(string key)
        => s_storageDictionary.TryAdd(key, null);

    public bool Contains(string key)
        => s_storageDictionary.ContainsKey(key);

    public void CopyFromScopedDictionary()
    {
        foreach (var (key, value) in _dictionary)
            s_storageDictionary[key] = value;
    }
}
