using System.Text;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.ManagementUi.ApiModels;
using GenericOutbox.ManagementUi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenericOutbox.ManagementUi.Controllers;

[ApiController]
[Route("Outbox/[controller]")]
public class OutboxEntityController : ControllerBase
{
    private readonly DbContext _dbContext;

    private readonly ILogger<OutboxEntityController> _logger;

    public OutboxEntityController(ILogger<OutboxEntityController> logger, IOutboxDbContextProvider outboxDbContextProvider)
    {
        _logger = logger;
        _dbContext = outboxDbContextProvider.DbContext;
    }

    [HttpGet]
    public async Task<ActionResult<OutboxEntityApiModel[]>> Get([FromQuery] OutboxEntityFilter filter)
    {
        var records = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .ApplyFilter(filter)
            .ToArrayAsync();

        return records
            .Select(ToApiModel)
            .ToArray();
    }

    [HttpPost]
    public async Task<ActionResult<OutboxEntityApiModel>> Post([FromBody] OutboxEntityCreateApiModel[] outboxEntities)
    {
        throw new NotImplementedException();
    }

    [HttpPut]
    public async Task<ActionResult<OutboxEntityApiModel[]>> Put([FromBody] OutboxEntityUpdateApiModel[] outboxEntities)
    {
        var ids = outboxEntities.Select(x => x.Id).ToArray();

        var records = await _dbContext.Set<OutboxEntity>()
            .AsTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        foreach (var outboxEntity in outboxEntities)
        {
            var record = records[outboxEntity.Id];
            record.Action = outboxEntity.Action;
            record.ParentId = outboxEntity.ParentId;
            record.Status = outboxEntity.Status;
            record.Lock = outboxEntity.Lock;

            if (record.PayloadString != outboxEntity.Payload)
                record.Payload = Encoding.UTF8.GetBytes(outboxEntity.Payload);

            record.Version = outboxEntity.Version;
            record.RetriesCount = outboxEntity.RetriesCount;
            record.RetryTimeoutUtc = outboxEntity.RetryTimeoutUtc;
        }

        await _dbContext.SaveChangesAsync();
        return records.Values.Select(ToApiModel).ToArray();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromQuery]int[] ids)
    {
        await _dbContext.Set<OutboxEntity>()
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync();

        return this.NoContent();
    }

    private OutboxEntityApiModel ToApiModel(OutboxEntity outboxEntity)
    {
        return new OutboxEntityApiModel
        {
            Action = outboxEntity.Action,
            Id = outboxEntity.Id,
            Lock = outboxEntity.Lock,
            Payload = outboxEntity.PayloadString,
            Status = outboxEntity.Status,
            Version = outboxEntity.Version,
            CreatedUtc = outboxEntity.CreatedUtc,
            ParentId = outboxEntity.ParentId,
            RetriesCount = outboxEntity.RetriesCount,
            ScopeId = outboxEntity.ScopeId,
            LastUpdatedUtc = outboxEntity.LastUpdatedUtc,
            RetryTimeoutUtc = outboxEntity.RetryTimeoutUtc,
        };
    }
}