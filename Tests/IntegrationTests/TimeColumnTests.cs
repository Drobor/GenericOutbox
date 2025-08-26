using System.Data.Common;
using System.Reflection;
using GenericOutbox;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Enums;
using IntegrationTests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class TimeColumnTests : DependencyInjectionTestBase
{
    private readonly IOutboxedKeyStorageService _outboxedKeyStorageService;
    private readonly IOutboxedOutboxTestHelperService _outboxTestHelperService;
    private readonly IOutboxCreatorContext _outboxCreatorContext;
    private readonly IntegrationTestsDbContext _dbContext;

    public TimeColumnTests()
    {
        _outboxedKeyStorageService = ServiceProvider.GetRequiredService<IOutboxedKeyStorageService>();
        _outboxCreatorContext = ServiceProvider.GetRequiredService<IOutboxCreatorContext>();
        _outboxTestHelperService = ServiceProvider.GetRequiredService<IOutboxedOutboxTestHelperService>();
        _dbContext = ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    [Fact]
    public async Task BasicFlowTest_ForOk()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        var beforeRecordWasCreated = DateTime.UtcNow;
        await _outboxTestHelperService.Delay(250);
        _outboxedKeyStorageService.Add(Guid.NewGuid().ToString());
        var afterRecordWasCreated = DateTime.UtcNow;

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId && x.Action.Contains("Add"));

        Assert.Equal(OutboxRecordStatus.ReadyToExecute, record.Status);
        Assert.Equal(record.CreatedUtc, record.LastUpdatedUtc);
        Assert.True(record.CreatedUtc >= beforeRecordWasCreated);
        Assert.True(record.CreatedUtc <= afterRecordWasCreated);

        await Task.Delay(1500);

        record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId && x.Action.Contains("Add"));

        Assert.Equal(OutboxRecordStatus.Completed, record.Status);
        Assert.NotEqual(record.CreatedUtc, record.LastUpdatedUtc);
        Assert.True(record.LastUpdatedUtc >= afterRecordWasCreated);
        Assert.True(record.LastUpdatedUtc <= DateTime.UtcNow);
    }

    [Fact]
    public async Task BasicFlowTest_ForFailed()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        var beforeRecordWasCreated = DateTime.UtcNow;
        await _outboxTestHelperService.Delay(250);
        _outboxTestHelperService.ThrowException();
        var afterRecordWasCreated = DateTime.UtcNow;

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId && x.Action.Contains("ThrowException"));

        Assert.Equal(OutboxRecordStatus.ReadyToExecute, record.Status);
        Assert.Equal(record.CreatedUtc, record.LastUpdatedUtc);
        Assert.True(record.CreatedUtc >= beforeRecordWasCreated);
        Assert.True(record.CreatedUtc <= afterRecordWasCreated);

        await Task.Delay(1500);

        record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId && x.Action.Contains("ThrowException"));

        Assert.Equal(OutboxRecordStatus.Failed, record.Status);
        Assert.NotEqual(record.CreatedUtc, record.LastUpdatedUtc);
        Assert.True(record.LastUpdatedUtc >= afterRecordWasCreated);
        Assert.True(record.LastUpdatedUtc <= DateTime.UtcNow);
    }
}