namespace GenericOutbox.ManagementUi.ApiModels;

public class OutboxEntityCreateApiModel
{
    public Guid? Lock { get; set; }
    public int? ParentId { get; set; }
    public Guid ScopeId { get; set; }
    public string Action { get; set; }
    public string Payload { get; set; }
    public int RetriesCount { get; set; }
    public string Version { get; set; }
}