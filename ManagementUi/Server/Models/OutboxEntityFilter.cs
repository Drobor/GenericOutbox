using GenericOutbox.Enums;

namespace GenericOutbox.ManagementUi.Models;

public class OutboxEntityFilter
{
    public int[]? Ids { get; set; }
    public OutboxRecordStatus? Status { get; set; }
    public string? ActionContains { get; set; }

    public int? Take { get; set; }
    public int? Skip { get; set; }
}