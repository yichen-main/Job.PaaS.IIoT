namespace IIoT.Domain.Shared.Businesses.Roots.Equipments;
public interface IEquipment : ITacticExpert
{
    const string Type = "equipment";
    ValueTask InstallAsync();
    Task<bool> IsExistEquipmentAsync(string equipmentNo);
    ValueTask AddAsync(Entity entity);
    ValueTask<Entity> AddAsync(string equipmentNo, Guid groupId, Guid networkId);
    ValueTask UpdateAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);
    Task<Entity> GetAsync(string equipmentNo);
    Task<INetwork.Entity> GetNetworkAsync(Guid equipmentId);
    Task<ScavengerScope> GetScavengerAsync(Guid equipmentId);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListGroupAsync(Guid id);
    Task<IEnumerable<Entity>> ListNetworkAsync(Guid id);
    IAsyncEnumerable<FactoryNetworkScope> ListFactoryNetworkAsync(Guid networkId);
    enum Status
    {
        Unused = 0,
        Run = 101,
        Idle = 102,
        Error = 103,
        Setup = 104,
        Shutdown = 105,
        Repair = 106,
        Maintenance = 107,
        Hold = 108
    }

    [StructLayout(LayoutKind.Auto)]
    readonly record struct FactoryNetworkScope
    {
        public required Entity Entity { get; init; }
        public required IFactoryGroup.Entity Group { get; init; }
        public required INetwork.Entity Network { get; init; }
    }
    readonly record struct ScavengerScope
    {
        public required IEnumerable<IMission.Entity> Missions { get; init; }
        public required IEnumerable<IEquipmentAlarm.Entity> Alarms { get; init; }
    }

    [Table(Name = $"{Deputy.Root}_{Type}_{Deputy.Foundation}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "group_id")] public required Guid GroupId { get; init; }
        [Field(Name = "network_id")] public required Guid NetworkId { get; init; }
        [Field(Name = "operate_type")] public required Operate OperateType { get; init; }
        [Field(Name = "equipment_no")] public required string EquipmentNo { get; init; }
        [Field(Name = "equipment_name")] public required string EquipmentName { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}