namespace IIoT.Domain.Shared.Businesses.Roots.Equipments;
public interface IOpcUaProcess : ITacticExpert
{
    ValueTask InstallAsync();
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id);

    [Table(Name = $"{Deputy.Equipment}_{INetworkOpcUa.Banner}_{Deputy.Process}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "equipment_id")] public required Guid EquipmentId { get; init; }
        [Field(Name = "node_path")] public required string NodePath { get; init; }
    }
    static string LinkNode => $"{INetworkOpcUa.Banner}_{nameof(Entity.EquipmentId).To<Entity>()}_{nameof(Entity.NodePath).To<Entity>()}_{Deputy.ComboLink}";
    string TableName { get; init; }
}