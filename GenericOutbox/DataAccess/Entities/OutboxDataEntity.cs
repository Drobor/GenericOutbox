namespace GenericOutbox.DataAccess.Entities;

public class OutboxDataEntity
{
    public int Id { get; set; }
    public Guid ScopeId { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }
}