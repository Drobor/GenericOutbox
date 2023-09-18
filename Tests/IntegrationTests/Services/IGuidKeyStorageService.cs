using GenericOutbox;

namespace IntegrationTests.Services;

[OutboxInterface]
public interface IOutboxedGuidKeyStorageService : IGuidKeyStorageService
{

}

public interface IGuidKeyStorageService : IGenericKeyStorageService<Guid>
{

}