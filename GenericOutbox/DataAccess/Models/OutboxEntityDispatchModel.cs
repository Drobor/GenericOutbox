using GenericOutbox.DataAccess.Entities;

namespace GenericOutbox.DataAccess.Models;

public record OutboxEntityDispatchModel(OutboxEntity OutboxEntity, bool StuckInProgress);