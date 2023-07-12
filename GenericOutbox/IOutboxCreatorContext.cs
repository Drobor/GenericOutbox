namespace GenericOutbox;

public interface IOutboxCreatorContext
{
    void CreateOutboxRecord<T>(string action, T model);
    Task CreateOutboxRecordAsync<T>(string action, T model);
    IDisposable Lock<T>(string entityName, T entityId);
    IDisposable Lock(Guid lockGuid);
}