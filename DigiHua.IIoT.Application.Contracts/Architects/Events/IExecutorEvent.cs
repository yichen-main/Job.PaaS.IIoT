namespace IIoT.Application.Contracts.Architects.Events;
public interface IExecutorEvent
{
    Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<InformationMission> missions);
    Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<ProductionMission> missions);
    Task BeginAsync(IMissionPush.EnvironmentType environment, IEnumerable<ParameterMission> missions);
    Task<IEnumerable<ProductionState>> SendAsync(string text, IMissionPush.EnvironmentType environment);
    ValueTask<Outcome> SendAsync(string text, IMissionPush.EnvironmentType environment, IWorkshopRawdata.EaiType eaiType);

    [StructLayout(LayoutKind.Auto)]
    readonly record struct InformationMission
    {
        public required IMissionPush.Entity Push { get; init; }
        public required IEquipment.Entity Equipment { get; init; }
        public required IInformationStack.Entity Stack { get; init; }
    }

    [StructLayout(LayoutKind.Auto)]
    readonly record struct ProductionMission
    {
        public required IMissionPush.Entity Push { get; init; }
        public required IEquipment.Entity Equipment { get; init; }
        public required IEnumerable<(IEstablishProduction.Entity process, IProductionStack.Entity stack)> Details { get; init; }
    }
    readonly record struct ParameterMission
    {
        public required IMissionPush.Entity Push { get; init; }
        public required IEquipment.Entity Equipment { get; init; }
        public required IEnumerable<(IEstablishParameter.Entity process, IParameterStack.Entity stack)> Details { get; init; }
    }
    readonly record struct ProductionState
    {
        public required string MachineNo { get; init; }
        public required string MachineName { get; init; }
        public required string MachineStatus { get; init; }
        public required string Description { get; init; }
        public required string StartTime { get; init; }
    }
    readonly record struct Outcome
    {
        public required string Endpoint { get; init; }
        public required string Message { get; init; }
        public required string Detail { get; init; }
        public required short ConsumeMS { get; init; }
    }
    readonly ref struct MESState
    {
        public const string ParameterKey = "workshop_machine_status";
        public const string ParameterDataName = "workshop_machine_status";
    }
    readonly ref struct ProcessEquipment
    {
        public const string ParameterKey = "change_machine_status";
        public const string ParameterDataName = "change_machine_status";
        public const string EquipmentNo = "machine_no";
        public const string EquipmentStatus = "machine_status";
        public const string EquipmentRemark = "remark";
        public const string ReportDateTime = "report_datetime";
    }
    readonly ref struct ProcessParameter
    {
        public const string ParameterKey = "parameter_check";
        public const string ParameterDataName = "parameter_check";
        public const string EquipmentNo = "machine_no";
        public const string AttributeNo = "attrib_no";
        public const string AttributeValue = "attrib_value";
        public const string ReportDateTime = "report_datetime";
    }
    readonly ref struct ProcessProduction
    {
        public const string ParameterKey = "production_edc";
        public const string ParameterDataName = "production_edc";
        public const string DetailName = "production_qty";
        public const string EquipmentNo = "machine_no";
        public const string AttributeNo = "attrib_no";
        public const string AttributeValue = "attrib_value";
        public const string ReportDateTime = "report_datetime";
        public const string PlotNo = "plot_no";
        public const string OpNo = "op_no";
        public const string ProdQty = "prod_qty";
    }
}