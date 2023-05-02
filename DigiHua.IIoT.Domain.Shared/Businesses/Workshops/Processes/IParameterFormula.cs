namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IParameterFormula : ITacticExpert
{
    const string Banner = "formula";
    ValueTask InstallAsync();
    ValueTask AddAsync(IEnumerable<Entity> entities);

    [Table(Name = $"{IProcessEstablish.Banner}_{IEstablishParameter.Banner}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "equipment_no")] public required string EquipmentNo { get; init; }
        [Field(Name = "data_no")] public required string DataNo { get; init; }
        [Field(Name = "data_value")] public required string DataValue { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}