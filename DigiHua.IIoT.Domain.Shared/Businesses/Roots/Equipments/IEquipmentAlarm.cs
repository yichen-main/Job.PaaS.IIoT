namespace IIoT.Domain.Shared.Businesses.Roots.Equipments;
public interface IEquipmentAlarm : ITacticExpert
{
    const string Banner = "alarm";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity);
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id);

    [Table(Name = $"{Deputy.Root}_{IEquipment.Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "equipment_id")] public required Guid EquipmentId { get; init; }
        [Field(Name = "exception_code")] public required string ExceptionCode { get; init; }
        [Field(Name = "description")] public required string Description { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}