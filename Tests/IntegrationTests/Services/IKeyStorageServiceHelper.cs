using GenericOutbox;

namespace IntegrationTests.Services;

[OutboxInterface]
public interface IOutboxedKeyStorageServiceHelper : IKeyStorageServiceHelper
{
}

public interface IKeyStorageServiceHelper
{
    void AddUsingScope(string key);
}
