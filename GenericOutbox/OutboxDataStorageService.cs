using System.Linq.Expressions;
using System.Reflection;
using GenericOutbox.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GenericOutbox;

public class OutboxDataStorageService<TDbContext> : IOutboxDataStorageService where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ISerializer _serializer;
    private readonly IOutboxHandlerContext _outboxHandlerContext;

    public OutboxDataStorageService(TDbContext dbContext, ISerializer serializer, IOutboxHandlerContext outboxHandlerContext)
    {
        _dbContext = dbContext;
        _serializer = serializer;
        _outboxHandlerContext = outboxHandlerContext;
    }

    public async Task Store<T>(string name, T data)
    {
        _dbContext.Set<OutboxDataEntity>().Add(
            new OutboxDataEntity
            {
                ScopeId = _outboxHandlerContext.ScopeId,
                Data = _serializer.Serialize(data),
                Name = name,
            });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<T?> Get<T>(string name)
    {
        var data = await _dbContext.Set<OutboxDataEntity>()
            .Where(x => x.ScopeId == _outboxHandlerContext.ScopeId && x.Name == name)
            .Select(x => x.Data)
            .FirstAsync();

        return _serializer.Deserialize<T>(data);
    }

    public Task<T?> GetOutboxStepResult<T>(string actionName)
        => Get<T>(ToOutboxStepResultName(actionName));

    public Task<T?> GetOutboxStepResult<T>(Type interfaceType, string methodName)
        => Get<T>(ToOutboxStepResultName($"{interfaceType.Name}.{methodName}"));

    public Task StoreOutboxStepResult<T>(string actionName, T data)
        => Store(ToOutboxStepResultName(actionName), data);

    private string ToOutboxStepResultName(string actionName)
        => $"outboxStepResult_{actionName}";
}