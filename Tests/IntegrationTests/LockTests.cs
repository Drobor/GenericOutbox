using GenericOutbox;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Enums;
using IntegrationTests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class LockTests : DependencyInjectionTestBase
{
    private readonly IKeyStorageService _keyStorageService;
    private readonly IntegrationTestsDbContext _dbContext;

    public LockTests()
    {
        _keyStorageService = ServiceProvider.GetRequiredService<IKeyStorageService>();
        _dbContext = ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    [Fact]
    public async Task FourLockedRecordsWithinTwoScope()
    {
        var outboxLockId = Guid.NewGuid();

        var outboxScopes = Enumerable
            .Range(0, 2)
            .Select(x => CreateLockedOutboxRecords(outboxLockId, 2))
            .ToArray();

        foreach (var scopeStorageKeys in outboxScopes)
        foreach (var storageKey in scopeStorageKeys)
            await ValidateKeyStorageContains(storageKey);
    }

    [Fact]
    public async Task FailedLockDoesntAffectAnotherLock()
    {
        var failedLockId = Guid.NewGuid();
        CreateLockedOutboxRecordsWithFailed(failedLockId,0, 2);
        await ValidateFailedOutboxRecordExistsAsync(failedLockId);

        var outboxLockId = Guid.NewGuid();

        var outboxScopes = Enumerable
            .Range(0, 2)
            .Select(x => CreateLockedOutboxRecords(outboxLockId, 2))
            .ToArray();

        foreach (var scopeStorageKeys in outboxScopes)
        foreach (var storageKey in scopeStorageKeys)
            await ValidateKeyStorageContains(storageKey);
    }

    [Fact]
    public async Task FailedLockDoesntLetFollowingRecordsToExecute()
    {
        var failedLockId = Guid.NewGuid();
        var scopeWithFailedRecord = CreateLockedOutboxRecordsWithFailed(failedLockId, 0, 2);

        var outboxScopes = Enumerable
            .Range(0, 2)
            .Select(x => CreateLockedOutboxRecords(failedLockId, 2))
            .ToArray();

        foreach (var storageKey in scopeWithFailedRecord)
            await ValidateKeyStorageDoesNotContain(storageKey);

        foreach (var scopeStorageKeys in outboxScopes)
        foreach (var storageKey in scopeStorageKeys)
            await ValidateKeyStorageDoesNotContain(storageKey);
    }

    [Fact]
    public async Task FailOnlyAffectsRecordsThatWereAddedAfterIt()
    {
        var countBeforeFailInFailedScope = 2;
        var countAfterFailInFailedScopet = 2;

        var failedLockId = Guid.NewGuid();
        var scopeBeforeFail = CreateLockedOutboxRecords(failedLockId, 2);
        var scopeWithFailedRecord = CreateLockedOutboxRecordsWithFailed(failedLockId, countBeforeFailInFailedScope, countAfterFailInFailedScopet);
        var scopeAfterFail = CreateLockedOutboxRecords(failedLockId, 2);

        foreach (var storageKey in scopeBeforeFail)
            await ValidateKeyStorageContains(storageKey);

        for (var i = 0; i < countBeforeFailInFailedScope; i++)
            await ValidateKeyStorageContains(scopeWithFailedRecord.Dequeue());

        for (var i = 0; i < countAfterFailInFailedScopet; i++)
            await ValidateKeyStorageDoesNotContain(scopeWithFailedRecord.Dequeue());

        foreach (var storageKey in scopeAfterFail)
            await ValidateKeyStorageDoesNotContain(storageKey);
    }


    private async Task ValidateFailedOutboxRecordExistsAsync(Guid lockId)
    {
        for (int i = 0; i < 10; i++)
        {
            if (await _dbContext.Set<OutboxEntity>().AnyAsync(x => x.Lock == lockId && x.Status == OutboxRecordStatus.Failed))
                return;

            await Task.Delay(100);
        }

        Assert.Fail($"Key storage didnt contain failed record with lock:{lockId}");
    }

    private async Task ValidateKeyStorageContains(string key)
    {
        for (int i = 0; i < 10; i++)
        {
            if (_keyStorageService.Contains(key))
                return;

            await Task.Delay(100);
        }

        Assert.Fail($"Key storage didnt contain {key}");
    }

    private async Task ValidateKeyStorageDoesNotContain(string key)
    {
        for (int i = 0; i < 10; i++)
        {
            if (_keyStorageService.Contains(key))
                Assert.Fail($"Key storage did contain {key}");

            await Task.Delay(100);
        }
    }


    private Queue<string> CreateLockedOutboxRecords(Guid lockId, int count)
    {
        using var scope = this.ServiceProvider.CreateScope();
        var outboxedKeyStorageService = scope.ServiceProvider.GetRequiredService<IOutboxedKeyStorageService>();
        var outboxCreatorContext = scope.ServiceProvider.GetRequiredService<IOutboxCreatorContext>();

        using var outboxLock = outboxCreatorContext.Lock(lockId);

        var result = new Queue<string>();

        for (var i = 0; i < count; i++)
        {
            var keyName = Guid.NewGuid().ToString();
            outboxedKeyStorageService.Add(keyName);
            result.Enqueue(keyName);
        }

        return result;
    }

    private Queue<string> CreateLockedOutboxRecordsWithFailed(Guid lockId, int countBeforeFail, int countAfterFail)
    {
        using var scope = this.ServiceProvider.CreateScope();
        var outboxedKeyStorageService = scope.ServiceProvider.GetRequiredService<IOutboxedKeyStorageService>();
        var outboxTestHelper = scope.ServiceProvider.GetRequiredService<IOutboxedOutboxTestHelperService>();
        var outboxCreatorContext = scope.ServiceProvider.GetRequiredService<IOutboxCreatorContext>();

        using var outboxLock = outboxCreatorContext.Lock(lockId);

        var result = new Queue<string>();

        for (var i = 0; i < countBeforeFail; i++)
        {
            var keyName = Guid.NewGuid().ToString();
            outboxedKeyStorageService.Add(keyName);
            result.Enqueue(keyName);
        }

        outboxTestHelper.ThrowException();

        for (var i = 0; i < countAfterFail; i++)
        {
            var keyName = Guid.NewGuid().ToString();
            outboxedKeyStorageService.Add(keyName);
            result.Enqueue(keyName);
        }

        return result;
    }
}