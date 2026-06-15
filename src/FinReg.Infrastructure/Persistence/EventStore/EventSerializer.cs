using System.Text.Json;
using FinReg.Domain.Events;

namespace FinReg.Infrastructure.Persistence.EventStore;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly Dictionary<string, Type> TypeMap = new()
    {
        [nameof(AccountOpenedEvent)] = typeof(AccountOpenedEvent),
        [nameof(AccountSuspendedEvent)] = typeof(AccountSuspendedEvent),
        [nameof(AccountClosedEvent)] = typeof(AccountClosedEvent),
        [nameof(AccountReviewInitiatedEvent)] = typeof(AccountReviewInitiatedEvent),
        [nameof(TransactionInitiatedEvent)] = typeof(TransactionInitiatedEvent),
        [nameof(TransactionCompletedEvent)] = typeof(TransactionCompletedEvent),
        [nameof(TransactionFailedEvent)] = typeof(TransactionFailedEvent),
        [nameof(TransactionFlaggedEvent)] = typeof(TransactionFlaggedEvent)
    };

    public static string Serialize(IDomainEvent @event) =>
        JsonSerializer.Serialize(@event, @event.GetType(), Options);

    public static IDomainEvent Deserialize(string eventType, string payload)
    {
        if (!TypeMap.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"Unknown event type: {eventType}");

        return (IDomainEvent)JsonSerializer.Deserialize(payload, type, Options)!;
    }
}
