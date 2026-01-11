using System.Text;
using GenericOutbox.DataAccess.Entities;
using GenericOutbox.Enums;
using GenericOutbox.ManagementUi;
using GenericOutbox.ManagementUi.App;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ManagementUiDbContext>(q => q.UseSqlite("Data Source=sqlite.db"));
builder.Services.AddOutboxManagementUi<ManagementUiDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.AddOutboxManagementUi();

app.MapControllers();

var migrationScope = app.Services.CreateScope();
var dbContext  = migrationScope.ServiceProvider.GetRequiredService<ManagementUiDbContext>();
await dbContext.Database.MigrateAsync();

dbContext.Set<OutboxEntity>().AddRange(Enumerable.Range(0, 20).Select(x => new OutboxEntity
{
    Version = "000",
    Status = OutboxRecordStatus.ReadyToExecute,
    Action = "SomeInt.SomeAct",
    Payload = Encoding.UTF8.GetBytes($"some payload:{Guid.NewGuid()}"),
    ScopeId = Guid.NewGuid(),
    CreatedUtc = DateTime.UtcNow,
    LastUpdatedUtc = DateTime.UtcNow
}));
await dbContext.SaveChangesAsync();

app.Run();