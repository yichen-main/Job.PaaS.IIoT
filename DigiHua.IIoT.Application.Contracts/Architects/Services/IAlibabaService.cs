namespace IIoT.Application.Contracts.Architects.Services;
public interface IAlibabaService
{
    const string FactoryNo = "Alibaba-Factory";
    const string GroupNo = "Alibaba-Group";
    Task PullAsync(string connectionNo, IQueueSection.Formula formula, IMqttClient entity);
    enum Status
    {
        Error = 1,
        Idle = 2,
        Run = 3,
        Shutdown = 8
    }
    readonly record struct Label
    {
        public const string TotalQuantity = "QTY";
    }
    readonly record struct Topic
    {
        public const string Information = "usr/custom/+/status";
        public const string Parameter = "usr/Module/DataDistribution/+/+/broadcast/+/+/metric/#";
    }
    readonly record struct Information
    {
        public required string AssetCode { get; init; }
        public required int Value { get; init; }
    }
    readonly record struct Parameter
    {
        public required Datum[] Data { get; init; }
        public readonly record struct Datum
        {
            public required string AssetCode { get; init; }
            public required string AttributeCode { get; init; }
            public required string AttributeName { get; init; }
            public required string AttributeGroupName { get; init; }
            public required object Value { get; init; }
            public required long Timestamp { get; init; }
            public required int Quality { get; init; }
        }
    }
}