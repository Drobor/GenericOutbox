namespace IntegrationTests.Services;

public interface IGenericKeyStorageService<TKey>
{
    bool Add(TKey key);
    bool Contains(TKey key);
}