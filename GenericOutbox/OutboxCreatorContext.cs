using System.Security.Cryptography;
using System.Text;
using GenericOutbox.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GenericOutbox;

class OutboxCreatorContext<TDbContext> : IOutboxCreatorContext where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ISerializer _serializer;
    private readonly OutboxOptions _outboxOptions;

    private readonly LockClearer _lockClearer;

    private int? _previousStepId;
    private Guid? _lock;
    private readonly Guid _scopeId;

    public OutboxCreatorContext(TDbContext dbContext, ISerializer serializer, OutboxOptions outboxOptions)
    {
        _dbContext = dbContext;
        _serializer = serializer;
        _outboxOptions = outboxOptions;

        _lockClearer = new LockClearer(this);
        _scopeId = Guid.NewGuid();
    }

    public void CreateOutboxRecord<T>(string action, T model)
    {
        var newRecord = CreateOutboxRecordInternal(action, model);
        _dbContext.SaveChanges();
        _previousStepId = newRecord.Id;
    }

    public async Task CreateOutboxRecordAsync<T>(string action, T model)
    {
        var newRecord = CreateOutboxRecordInternal(action, model);
        await _dbContext.SaveChangesAsync();
        _previousStepId = newRecord.Id;
    }

    public IDisposable Lock<T>(string entityName, T entityId)
    {
        var lockStr = $"{entityName.Replace(":", "::")}:{entityId.ToString().Replace(":", "::")}";
        //ToDo: store lock expaination
        return Lock(new Guid(MD5.HashData(Encoding.UTF8.GetBytes(lockStr))));
    }

    public IDisposable Lock(Guid lockGuid)
    {
        _lock = lockGuid;
        return _lockClearer;
    }

    private OutboxEntity CreateOutboxRecordInternal<T>(string action, T model)
    {
        var utcNow = DateTime.UtcNow;

        var newRecord = new OutboxEntity
        {
            Action = action,
            ScopeId = _scopeId,
            Payload = _serializer.Serialize(model),
            ParentId = _previousStepId,
            Version = _outboxOptions.Version,
            Lock = _lock,
            LastUpdatedUtc = utcNow,
            CreatedUtc = utcNow
        };

        _dbContext.Set<OutboxEntity>().Add(newRecord);

        return newRecord;
    }

    class LockClearer : IDisposable
    {
        private readonly OutboxCreatorContext<TDbContext> _outboxCreatorContext;

        public LockClearer(OutboxCreatorContext<TDbContext> outboxCreatorContext)
        {
            _outboxCreatorContext = outboxCreatorContext;
        }

        public void Dispose()
        {
            _outboxCreatorContext._lock = null;
        }
    }
}