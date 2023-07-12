namespace GenericOutbox;

class OutboxHandlerContext : IOutboxHandlerContext
{
    public Guid ScopeId { get; set; }
}