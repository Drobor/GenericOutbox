using GenericOutbox.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericOutbox.DataAccess.Configuration;

public class OutboxEntityConfiguration : IEntityTypeConfiguration<OutboxEntity>
{
    public void Configure(EntityTypeBuilder<OutboxEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Version);
        builder.HasIndex(x => x.HandlerLock);
        builder.HasIndex(x => x.Lock);
        builder.HasIndex(x => x.ScopeId);
    }
}

public class OutboxDataEntityConfiguration : IEntityTypeConfiguration<OutboxDataEntity>
{
    public void Configure(EntityTypeBuilder<OutboxDataEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ScopeId);
    }
}
