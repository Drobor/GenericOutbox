using GenericOutbox;

namespace IntegrationTests.Services;

[OutboxInterface]
public interface IOutboxedKeyStorageService : IKeyStorageService
{
}

public interface IKeyStorageService
{
    bool Add(string key);
    bool Contains(string key);
}