namespace GenericOutbox;

public interface IOutboxDataStorageService
{
    Task Store<T>(string name, T data);
    Task<T?> Get<T>(string name);
    Task<T?> GetOutboxStepResult<T>(string actionName);
    Task StoreOutboxStepResult<T>(string actionName, T data);
}