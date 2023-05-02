namespace IIoT.Domain.Shared.Businesses.Roots.Factories;
public interface IFactoryGroup : ITacticExpert
{
    const string Banner = "group";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity);
    ValueTask UpdateAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);
    Task<Entity> GetAsync(string groupNo);
    Task<SingleFactory> GetSingleFactoryAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListFactoryAsync(Guid id);
    IAsyncEnumerable<(IFactory.Entity factory, Entity group)> ListFactoryGroupAsync();
    IAsyncEnumerable<(INetwork.Entity network, IEquipment.Entity equipment)> ListNetworkEquipmentAsync(Guid id);

    [StructLayout(LayoutKind.Auto)]
    readonly record struct SingleFactory
    {
        public required Entity Entity { get; init; }
        public required IFactory.Entity Factory { get; init; }
    }

    [Table(Name = $"{Deputy.Root}_{IFactory.Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "factory_id")] public required Guid FactoryId { get; init; }
        [Field(Name = "group_no")] public required string GroupNo { get; init; }
        [Field(Name = "group_name")] public required string GroupName { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}