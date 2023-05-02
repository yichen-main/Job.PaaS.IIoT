namespace IIoT.Domain.Shared.Functions.Rawdatas;
public interface IWorkshopRawdata
{
    const int Day = 86400;
    ValueTask BuildAsync();
    ValueTask InsertAsync(Title title, IEquipment.Status ststus);
    ValueTask InsertAsync(Title title, IEnumerable<Production.Meta> texts);
    ValueTask InsertAsync(Title title, IEnumerable<Parameter.Meta> texts);
    ValueTask WriteAsync<TEntity>(TEntity entity) where TEntity : Timeseries;
    ValueTask WriteAsync<TEntity>(TEntity[] entities) where TEntity : Timeseries;
    IDictionary<string, TEntity[]> Read<TEntity>(IProcessEstablish.ProcessType type, DateTimeOffset start, DateTimeOffset end) where TEntity : Timeseries;
    enum EaiType
    {
        [Description("change.machine.status.process")] Information = 1001,
        [Description("parameter.check.process")] Parameter = 1002,
        [Description("production.edc.process")] Production = 1003
    }
    readonly record struct Title
    {
        public required string SourceNo { get; init; }
        public required string FactoryNo { get; init; }
        public required string GroupNo { get; init; }
        public required string EquipmentNo { get; init; }
    }

    [Measurement("equipments_informations")]
    sealed class Information : Timeseries
    {
        [Column("status")] public required byte Status { get; init; }
        public readonly record struct Meta
        {
            public required string Status { get; init; }
        }
    }

    [Measurement("equipments_productions")]
    sealed class Production : Timeseries
    {
        [Column("dispatch_no", IsTag = true)] public required string DispatchNo { get; init; } = string.Empty;
        [Column("batch_no", IsTag = true)] public required string BatchNo { get; init; } = string.Empty;
        [Column("output")] public required int Output { get; init; }
        public record struct Meta
        {
            public required string DispatchNo { get; init; }
            public required string BatchNo { get; init; }
            public required int Output { get; set; }
        }
    }

    [Measurement("equipments_parameters")]
    sealed class Parameter : Timeseries
    {
        [Column("data_no", IsTag = true)] public required string DataNo { get; init; }
        [Column("data_value")] public required float DataValue { get; init; }
        public readonly record struct Meta
        {
            public required string DataNo { get; init; }
            public required float DataValue { get; init; }
        }
        public readonly record struct Universal
        {
            public required string DataNo { get; init; }
            public required object DataValue { get; init; }
        }
    }
    abstract class Timeseries
    {
        [Column(IsTimestamp = true)] public required DateTime Timestamp { get; init; }
        [Column("identify_no", IsTag = true)] public required ushort IdentifyNo { get; init; }
        [Column("workshop_no", IsTag = true)] public required string WorkshopNo { get; init; }
    }
    static string HeadName => "workshop_processes";
}