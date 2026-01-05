using System.Reflection;
using GenericOutbox;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Enums;
using IntegrationTests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class StuckInProgressTests : DependencyInjectionTestBase
{
    private readonly IOutboxedOutboxTestHelperService _outboxTestHelperService;
    private readonly IOutboxCreatorContext _outboxCreatorContext;
    private readonly IntegrationTestsDbContext _dbContext;

    public StuckInProgressTests()
    {
        ServiceProvider.GetRequiredService<IOutboxedKeyStorageService>();
        _outboxCreatorContext = ServiceProvider.GetRequiredService<IOutboxCreatorContext>();
        _outboxTestHelperService = ServiceProvider.GetRequiredService<IOutboxedOutboxTestHelperService>();
        _dbContext = ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    [Fact]
    public async Task StuckInProgress_NothingHappensUntilTimeout()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.Delay(999999);
        await Task.Delay(4900);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.InProgress, record.Status);
    }

    [Fact]
    public async Task StuckInProgress_HandlesWithFail()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.StuckInProgressToFail();
        await Task.Delay(6500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.Failed, record.Status);
    }

    [Fact]
    public async Task StuckInProgress_HandlesWithComplete()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.StuckInProgressToComplete();
        await Task.Delay(6500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.Completed, record.Status);
    }

    [Fact]
    public async Task StuckInProgress_HandlesWithRetry()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.StuckInProgressToRetry();
        await Task.Delay(6500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.WaitingForRetry, record.Status);
    }
}