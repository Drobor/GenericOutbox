using System.Text;
using GenericOutbox.Enums;

namespace GenericOutbox.DataAccess.Entities;

public class OutboxEntity
{
    public int Id { get; set; }
    public Guid? Lock { get; set; }
    public Guid? HandlerLock { get; set; }
    public int? ParentId { get; set; }
    public Guid ScopeId { get; set; }
    public string Action { get; set; }
    public byte[] Payload { get; set; }
    public OutboxRecordStatus Status { get; set; }
    public DateTime? RetryTimeoutUtc { get; set; }
    public int RetriesCount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string Version { get; set; }

    public virtual OutboxEntity Parent { get; set; }

    public string PayloadString => Encoding.UTF8.GetString(Payload);
}