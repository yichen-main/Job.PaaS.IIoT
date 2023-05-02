namespace IIoT.Domain.Shared.Businesses.Workshops.Missions;
public interface IMissionPush : ITacticExpert
{
    const string Banner = "push";
    ValueTask InstallAsync();
    Task<int> GaugeAsync();
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id);
    enum EnvironmentType
    {
        Production = 101,
        Experiment = 102
    }

    [StructLayout(LayoutKind.Auto), Table(Name = $"{Deputy.Workshop}_{Deputy.Mission}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "operate_type")] public required Operate OperateType { get; init; }
        [Field(Name = "environment_type")] public required EnvironmentType EnvironmentType { get; init; }
    }
    string TableName { get; init; }
}