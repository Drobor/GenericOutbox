namespace GenericOutbox;

public interface IOutboxServiceLocator
{
    IServiceProvider? ServiceProvider { get; internal set; }
}