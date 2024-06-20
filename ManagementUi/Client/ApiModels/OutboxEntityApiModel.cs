namespace GenericOutbox.ManagementUi.Client.ApiModels;

public class OutboxEntityApiModel
{
    public int Id { get; set; }
    public Guid? Lock { get; set; }
    public int? ParentId { get; set; }
    public Guid ScopeId { get; set; }
    public string Action { get; set; }
    public string Payload { get; set; }
    public OutboxRecordStatus Status { get; set; }
    public DateTime? RetryTimeoutUtc { get; set; }
    public int RetriesCount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string Version { get; set; }
}