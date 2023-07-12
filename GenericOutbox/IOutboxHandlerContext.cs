namespace GenericOutbox;

public interface IOutboxHandlerContext
{
    Guid ScopeId { get; set; }
}