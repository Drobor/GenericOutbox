using System.Collections.Concurrent;

namespace IntegrationTests.Services;

public class GenericKeyStorageService<TKey> : IGenericKeyStorageService<TKey>
{
    private static readonly ConcurrentDictionary<TKey, object> s_storageDictionary = new ConcurrentDictionary<TKey, object>();

    private readonly Dictionary<string, string> _dictionary;

    public bool Add(TKey key)
        => s_storageDictionary.TryAdd(key, null);

    public bool Contains(TKey key)
        => s_storageDictionary.ContainsKey(key);
}

public class GuidKeyStorageService : GenericKeyStorageService<Guid>, IGuidKeyStorageService
{

}