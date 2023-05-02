namespace IIoT.Domain.Shared.Businesses.Roots.Equipments;
public interface IProduceState : ITacticExpert
{
    const string Banner = "state";
    const string Type = "produce";
    ValueTask InstallAsync();
    ValueTask UpsertAsync(Entity entity);
    ValueTask UpsertAsync(IEnumerable<Entity> entities);
    Task<IEnumerable<Entity>> ListAsync();

    [Table(Name = $"{Deputy.Equipment}_{Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = "equipment_no", PK = true)] public required string EquipmentNo { get; init; }
        [Field(Name = "equipment_name")] public required string EquipmentName { get; init; }
        [Field(Name = "equipment_status")] public required IEquipment.Status EquipmentStatus { get; init; }
        [Field(Name = "environment_type")] public required IMissionPush.EnvironmentType EnvironmentType { get; init; }
        [Field(Name = "description")] public required string Description { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}