namespace IIoT.Domain.Shared.Businesses.Workshops.Missions;
public interface IPushHistory : ITacticExpert
{
    const string Banner = "history";
    ValueTask InstallAsync();
    Task<int> GaugeAsync(string filter);
    ValueTask AddAsync(Entity entity, IEnumerable<IInformationStack.Entity> stacks);
    ValueTask AddAsync(Entity entity, IEnumerable<IProductionStack.Entity> stacks);
    ValueTask AddAsync(Entity entity, IEnumerable<IParameterStack.Entity> stacks);
    Task<Entity> GetAsync(Guid id);
    Task<IEnumerable<Entity>> ListAsync(string format, IEnumerable<(string field, string value)> filter);
    enum Comparison
    {
        Equal,
        Exceed,
        NotEqual,
        Include,
        GreaterOrEqualTo,
        LessThan,
        LessThanOrEqualTo
    }
    enum QueryCondition
    {
        ProductionEnvironment = 101
    }
    readonly record struct InformationRecord
    {
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required IEquipment.Status Status { get; init; }
        public required DateTime EventTime { get; init; }
    }
    readonly record struct ProductionRecord
    {
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required IEnumerable<Content> Contents { get; init; }

        [StructLayout(LayoutKind.Auto)]
        public readonly record struct Content
        {
            public required string DispatchNo { get; init; }
            public required string BatchNo { get; init; }
            public required int Output { get; init; }
            public required DateTime EventTime { get; init; }
        }
    }
    readonly record struct ParameterRecord
    {
        public required string EquipmentNo { get; init; }
        public required string EquipmentName { get; init; }
        public required IEnumerable<Content> Contents { get; init; }
        public readonly record struct Content
        {
            public required Guid Id { get; init; }
            public required string DataNo { get; init; }
            public required float DataValue { get; init; }
            public required DateTime EventTime { get; init; }
        }
    }

    [Table(Name = $"{Deputy.Mission}_{IMissionPush.Banner}_{Banner}")]
    readonly record struct Entity
    {
        [Field(Name = CurrentSign, PK = true)] public required Guid Id { get; init; }
        [Field(Name = "eai_type")] public required IWorkshopRawdata.EaiType EaiType { get; init; }
        [Field(Name = "environment_type")] public required IMissionPush.EnvironmentType EnvironmentType { get; init; }
        [Field(Name = "content_record")] public required string ContentRecord { get; init; }
        [Field(Name = "result_record")] public required string ResultRecord { get; init; }
        [Field(Name = "consume_ms")] public required short ConsumeMS { get; init; }
        [Field(Name = "create_time")] public required DateTime CreateTime { get; init; }
    }
    string TableName { get; init; }
}