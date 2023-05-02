namespace IIoT.Domain.Shared.Businesses.Roots.Networks;
public interface INetwork : ITacticExpert
{
    const string Type = "network";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity);
    ValueTask AddAsync(Entity entity, INetworkMqtt.Entity detail);
    ValueTask AddAsync(Entity entity, INetworkOpcUa.Entity detail);
    ValueTask UpdateAsync(Entity entity);
    ValueTask UpdateAsync(Entity entity, (Guid id, string table) remove);
    ValueTask UpdateAsync(Entity entity, INetworkMqtt.Entity detail);
    ValueTask UpdateAsync(Entity entity, INetworkOpcUa.Entity detail);
    ValueTask UpdateAsync(Entity entity, INetworkMqtt.Entity insert, (Guid id, string table) remove);
    ValueTask UpdateAsync(Entity entity, INetworkOpcUa.Entity insert, (Guid id, string table) remove);
    ValueTask UpdateChangeAsync(Entity entity, INetworkMqtt.Entity insert);
    ValueTask UpdateChangeAsync(Entity entity, INetworkOpcUa.Entity insert);
    Task<Entity> GetAsync(Guid id);
    Task<Entity> GetAsync(string sessionNo);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListAsync(Category category);
    enum Status
    {
        Unused = 100,
        Connected = 101,
        Disconnected = 102
    }
    enum Category
    {
        PassiveReception = 101,
        MessageQueuingTelemetryTransport = 102,
        OPCUnifiedArchitecture = 103
    }

    [Table(Name = $"{Deputy.Root}_{Type}_{Deputy.Foundation}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "category_type")] public required Category CategoryType { get; init; }
        [Field(Name = "session_no")] public required string SessionNo { get; init; }
        [Field(Name = "session_name")] public required string SessionName { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}