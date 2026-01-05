using System.Reflection;
using GenericOutbox;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Enums;
using IntegrationTests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class RetryTests : DependencyInjectionTestBase
{
    private readonly IOutboxedOutboxTestHelperService _outboxTestHelperService;
    private readonly IOutboxCreatorContext _outboxCreatorContext;
    private readonly IntegrationTestsDbContext _dbContext;

    public RetryTests()
    {
        ServiceProvider.GetRequiredService<IOutboxedKeyStorageService>();
        _outboxCreatorContext = ServiceProvider.GetRequiredService<IOutboxCreatorContext>();
        _outboxTestHelperService = ServiceProvider.GetRequiredService<IOutboxedOutboxTestHelperService>();
        _dbContext = ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    [Fact]
    public async Task Retry_HandlesWithFail()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.ThrowToFail();
        await Task.Delay(1500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.Failed, record.Status);
    }

    [Fact]
    public async Task Retry_HandlesWithComplete()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.ThrowToSuccess();
        await Task.Delay(1500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.Completed, record.Status);
    }

    [Fact]
    public async Task Retry_HandlesWithRetry()
    {
        var scopeId = (Guid)_outboxCreatorContext.GetType().GetField("_scopeId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(_outboxCreatorContext);
        await _outboxTestHelperService.ThrowToRetry();
        await Task.Delay(1500);

        var record = await _dbContext.Set<OutboxEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.ScopeId == scopeId);

        Assert.Equal(OutboxRecordStatus.WaitingForRetry, record.Status);
    }
}