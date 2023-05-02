namespace IIoT.Domain.Shared.Businesses.Workshops.Processes;
public interface IProcessEstablish : ITacticExpert
{
    const string Type = "process";
    const string Banner = "establish";
    ValueTask InstallAsync();
    ValueTask AddAsync(Entity entity, IEstablishInformation.Entity process);
    ValueTask AddAsync(Entity entity, IEstablishInformation.Entity process, IOpcUaProcess.Entity detail);
    ValueTask AddAsync(Entity entity, IEnumerable<IEstablishProduction.Entity> processes);
    ValueTask AddAsync(Entity entity, IEnumerable<(IEstablishProduction.Entity process, IOpcUaProcess.Entity opcUa)> details);
    ValueTask AddAsync(Entity entity, IEnumerable<IEstablishParameter.Entity> processes);
    ValueTask AddAsync(Entity entity, IEnumerable<(IEstablishParameter.Entity process, IOpcUaProcess.Entity opcUa)> details);
    ValueTask UpdateAsync(Entity entity, IEstablishInformation.Entity process);
    ValueTask UpdateAsync(Entity entity, IEstablishInformation.Entity process, IOpcUaProcess.Entity detail);
    ValueTask UpdateAsync(Entity entity, IEstablishProduction.Entity process);
    ValueTask UpdateAsync(Entity entity, IEstablishProduction.Entity process, IOpcUaProcess.Entity opcUa);
    ValueTask UpdateAsync(Entity entity, IEstablishParameter.Entity process);
    ValueTask UpdateAsync(Entity entity, IEstablishParameter.Entity process, IOpcUaProcess.Entity opcUa);
    Task<Entity> GetAsync(Guid id);
    Task<WorkshopData> GetWorkshopDataAsync(Guid establishId);
    Task<IEnumerable<Entity>> ListAsync();
    Task<IEnumerable<Entity>> ListEquipmentAsync(Guid id);
    public enum ProcessType
    {
        EquipmentStatus = 1001,
        EquipmentParameter = 1002,
        EquipmentOutput = 1003
    }
    readonly record struct WorkshopData
    {
        public required Entity Entity { get; init; }
        public required IEstablishInformation.Entity Information { get; init; }
        public required IEnumerable<IEstablishParameter.Entity> Parameters { get; init; }
        public required IEnumerable<IEstablishProduction.Entity> Productions { get; init; }
    }

    [Table(Name = $"{Deputy.Workshop}_{Type}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "equipment_id")] public required Guid EquipmentId { get; init; }
        [Field(Name = "process_type")] public required ProcessType ProcessType { get; init; }
        [Field(Name = "creator")] public required string Creator { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}